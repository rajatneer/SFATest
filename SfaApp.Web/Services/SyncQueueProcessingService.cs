using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Services;

public class SyncQueueProcessingService : ISyncQueueProcessingService
{
    private const int RetryIntervalMinutes = 5;
    private const int MaxRetryCount = 12;

    private readonly ApplicationDbContext _dbContext;
    private readonly ISyncTransportClient _syncTransportClient;
    private readonly ILogger<SyncQueueProcessingService> _logger;

    public SyncQueueProcessingService(
        ApplicationDbContext dbContext,
        ISyncTransportClient syncTransportClient,
        ILogger<SyncQueueProcessingService> logger)
    {
        _dbContext = dbContext;
        _syncTransportClient = syncTransportClient;
        _logger = logger;
    }

    public async Task<int> ProcessDueItemsAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var retryCutoff = utcNow.AddMinutes(-RetryIntervalMinutes);

        var dueItems = await _dbContext.SyncQueueItems
            .Where(x =>
                x.SyncStatus == SyncStatus.Pending ||
                (x.SyncStatus == SyncStatus.Failed &&
                 x.RetryCount < MaxRetryCount &&
                 (!x.LastRetryAtUtc.HasValue || x.LastRetryAtUtc <= retryCutoff)))
            .OrderBy(x => x.CreatedAtUtc)
            .Take(25)
            .ToListAsync(cancellationToken);

        var processedCount = 0;
        foreach (var item in dueItems)
        {
            await ProcessItemAsync(item, utcNow, cancellationToken);
            processedCount++;
        }

        return processedCount;
    }

    private async Task ProcessItemAsync(SyncQueueItem item, DateTime utcNow, CancellationToken cancellationToken)
    {
        try
        {
            using var payload = JsonDocument.Parse(item.PayloadJson);
            var localState = await GetLocalStateAsync(item, cancellationToken);

            if (localState == SyncLocalState.Missing)
            {
                throw new InvalidOperationException($"Sync target not found for entity {item.EntityType} and uuid {item.EntityClientUuid}.");
            }

            SyncTransportResult transportResult;
            if (localState == SyncLocalState.AlreadySynced)
            {
                transportResult = SyncTransportResult.Conflict("Conflict resolved using server-wins strategy.");
            }
            else
            {
                transportResult = await _syncTransportClient.PushAsync(item, payload, cancellationToken);
                if (!transportResult.IsSuccess && !transportResult.IsConflict)
                {
                    await MarkFailureAsync(item.Id, utcNow, transportResult.Message ?? "External sync transport failed.", cancellationToken);
                    return;
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (localState == SyncLocalState.Pending)
            {
                await MarkLocalEntitySyncedAsync(item, cancellationToken);
            }

            item.SyncStatus = SyncStatus.Synced;
            item.RetryCount = 0;
            item.LastRetryAtUtc = utcNow;
            item.LastErrorMessage = transportResult.IsConflict
                ? (transportResult.Message ?? "Conflict resolved using server-wins strategy.")
                : null;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Sync queue item {SyncQueueItemId} failed with database error", item.Id);
            await MarkFailureAsync(item.Id, utcNow, ex.ToString(), cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Sync queue item {SyncQueueItemId} failed with invalid operation", item.Id);
            await MarkFailureAsync(item.Id, utcNow, ex.ToString(), cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Sync queue item {SyncQueueItemId} has invalid payload JSON", item.Id);
            await MarkFailureAsync(item.Id, utcNow, ex.ToString(), cancellationToken);
        }
    }

    private async Task<SyncLocalState> GetLocalStateAsync(SyncQueueItem item, CancellationToken cancellationToken)
    {
        switch (item.EntityType)
        {
            case "DaySession":
                return await GetDaySessionStateAsync(item.EntityClientUuid, cancellationToken);
            case "Visit":
                return await GetVisitStateAsync(item.EntityClientUuid, cancellationToken);
            case "SalesOrder":
                return await GetSalesOrderStateAsync(item.EntityClientUuid, cancellationToken);
            default:
                throw new InvalidOperationException($"Unsupported sync entity type: {item.EntityType}");
        }
    }

    private async Task MarkLocalEntitySyncedAsync(SyncQueueItem item, CancellationToken cancellationToken)
    {
        switch (item.EntityType)
        {
            case "DaySession":
                await MarkDaySessionSyncedAsync(item.EntityClientUuid, cancellationToken);
                break;
            case "Visit":
                await MarkVisitSyncedAsync(item.EntityClientUuid, cancellationToken);
                break;
            case "SalesOrder":
                await MarkSalesOrderSyncedAsync(item.EntityClientUuid, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported sync entity type: {item.EntityType}");
        }
    }

    private async Task<SyncLocalState> GetDaySessionStateAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var session = await _dbContext.DaySessions.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (session is null)
        {
            return SyncLocalState.Missing;
        }

        return session.SyncStatus == SyncStatus.Synced
            ? SyncLocalState.AlreadySynced
            : SyncLocalState.Pending;
    }

    private async Task MarkDaySessionSyncedAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var session = await _dbContext.DaySessions.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException("Day session not found for sync payload.");
        }

        session.SyncStatus = SyncStatus.Synced;
    }

    private async Task<SyncLocalState> GetVisitStateAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var visit = await _dbContext.Visits.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (visit is null)
        {
            return SyncLocalState.Missing;
        }

        return visit.SyncStatus == SyncStatus.Synced
            ? SyncLocalState.AlreadySynced
            : SyncLocalState.Pending;
    }

    private async Task MarkVisitSyncedAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var visit = await _dbContext.Visits.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (visit is null)
        {
            throw new InvalidOperationException("Visit not found for sync payload.");
        }

        visit.SyncStatus = SyncStatus.Synced;
    }

    private async Task<SyncLocalState> GetSalesOrderStateAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var order = await _dbContext.SalesOrders.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (order is null)
        {
            return SyncLocalState.Missing;
        }

        return order.SyncStatus == SyncStatus.Synced
            ? SyncLocalState.AlreadySynced
            : SyncLocalState.Pending;
    }

    private async Task MarkSalesOrderSyncedAsync(string entityClientUuid, CancellationToken cancellationToken)
    {
        var order = await _dbContext.SalesOrders.FirstOrDefaultAsync(x => x.ClientGeneratedUuid == entityClientUuid, cancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException("Sales order not found for sync payload.");
        }

        order.SyncStatus = SyncStatus.Synced;
    }

    private async Task MarkFailureAsync(int itemId, DateTime utcNow, string errorDetails, CancellationToken cancellationToken)
    {
        var item = await _dbContext.SyncQueueItems.FirstOrDefaultAsync(x => x.Id == itemId, cancellationToken);
        if (item is null)
        {
            return;
        }

        item.RetryCount += 1;
        item.LastRetryAtUtc = utcNow;
        item.SyncStatus = SyncStatus.Failed;

        if (item.RetryCount >= MaxRetryCount)
        {
            item.LastErrorMessage = $"Max retry count reached ({MaxRetryCount}). {errorDetails}";
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        item.LastErrorMessage = errorDetails;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private enum SyncLocalState
    {
        Missing,
        Pending,
        AlreadySynced
    }
}
