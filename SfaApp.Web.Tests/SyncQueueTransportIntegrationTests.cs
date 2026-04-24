using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Services;
using SfaApp.Web.Tests.Infrastructure;

namespace SfaApp.Web.Tests;

[Trait("Category", "Integration")]
public class SyncQueueTransportIntegrationTests
{
    [Fact]
    public async Task ProcessDueItems_WhenTransportConflict_MarksQueueAndEntitySynced()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entityUuid = $"day-{Guid.NewGuid():N}";
        dbContext.DaySessions.Add(new DaySession
        {
            RepUserId = "rep-1",
            BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDayTimestampUtc = DateTime.UtcNow,
            StartDayLat = 12.34m,
            StartDayLong = 56.78m,
            ClientGeneratedUuid = entityUuid,
            SyncStatus = SyncStatus.Pending
        });

        dbContext.SyncQueueItems.Add(new SyncQueueItem
        {
            EntityType = "DaySession",
            EntityClientUuid = entityUuid,
            PayloadJson = "{}",
            SyncStatus = SyncStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var fakeTransport = new FakeSyncTransportClient(SyncTransportResult.Conflict("remote conflict"));
        var service = new SyncQueueProcessingService(dbContext, fakeTransport, NullLogger<SyncQueueProcessingService>.Instance);

        var processed = await service.ProcessDueItemsAsync();

        Assert.Equal(1, processed);
        Assert.Equal(1, fakeTransport.CallCount);

        var item = await dbContext.SyncQueueItems.SingleAsync();
        var daySession = await dbContext.DaySessions.SingleAsync();

        Assert.Equal(SyncStatus.Synced, item.SyncStatus);
        Assert.Equal(0, item.RetryCount);
        Assert.Contains("conflict", item.LastErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SyncStatus.Synced, daySession.SyncStatus);
    }

    [Fact]
    public async Task ProcessDueItems_WhenTransportFails_IncrementsRetryAndKeepsEntityPending()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entityUuid = $"day-{Guid.NewGuid():N}";
        dbContext.DaySessions.Add(new DaySession
        {
            RepUserId = "rep-1",
            BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDayTimestampUtc = DateTime.UtcNow,
            StartDayLat = 10.11m,
            StartDayLong = 12.13m,
            ClientGeneratedUuid = entityUuid,
            SyncStatus = SyncStatus.Pending
        });

        dbContext.SyncQueueItems.Add(new SyncQueueItem
        {
            EntityType = "DaySession",
            EntityClientUuid = entityUuid,
            PayloadJson = "{}",
            SyncStatus = SyncStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var fakeTransport = new FakeSyncTransportClient(SyncTransportResult.Failure("network timeout"));
        var service = new SyncQueueProcessingService(dbContext, fakeTransport, NullLogger<SyncQueueProcessingService>.Instance);

        var processed = await service.ProcessDueItemsAsync();

        Assert.Equal(1, processed);
        Assert.Equal(1, fakeTransport.CallCount);

        var item = await dbContext.SyncQueueItems.SingleAsync();
        var daySession = await dbContext.DaySessions.SingleAsync();

        Assert.Equal(SyncStatus.Failed, item.SyncStatus);
        Assert.Equal(1, item.RetryCount);
        Assert.Contains("network timeout", item.LastErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SyncStatus.Pending, daySession.SyncStatus);
    }

    [Fact]
    public async Task ProcessDueItems_WhenFailedItemNotDue_DoesNotRetryYet()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entityUuid = $"day-{Guid.NewGuid():N}";
        dbContext.DaySessions.Add(new DaySession
        {
            RepUserId = "rep-1",
            BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDayTimestampUtc = DateTime.UtcNow,
            StartDayLat = 22.22m,
            StartDayLong = 33.33m,
            ClientGeneratedUuid = entityUuid,
            SyncStatus = SyncStatus.Pending
        });

        dbContext.SyncQueueItems.Add(new SyncQueueItem
        {
            EntityType = "DaySession",
            EntityClientUuid = entityUuid,
            PayloadJson = "{}",
            SyncStatus = SyncStatus.Failed,
            RetryCount = 1,
            LastRetryAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var fakeTransport = new FakeSyncTransportClient(SyncTransportResult.Success());
        var service = new SyncQueueProcessingService(dbContext, fakeTransport, NullLogger<SyncQueueProcessingService>.Instance);

        var processed = await service.ProcessDueItemsAsync();

        Assert.Equal(0, processed);
        Assert.Equal(0, fakeTransport.CallCount);

        var item = await dbContext.SyncQueueItems.SingleAsync();
        Assert.Equal(1, item.RetryCount);
        Assert.Equal(SyncStatus.Failed, item.SyncStatus);
    }

    private sealed class FakeSyncTransportClient : ISyncTransportClient
    {
        private readonly Queue<SyncTransportResult> _results;

        public FakeSyncTransportClient(params SyncTransportResult[] results)
        {
            _results = new Queue<SyncTransportResult>(results.Length > 0
                ? results
                : [SyncTransportResult.Success()]);
        }

        public int CallCount { get; private set; }

        public Task<SyncTransportResult> PushAsync(SyncQueueItem item, JsonDocument payload, CancellationToken cancellationToken = default)
        {
            CallCount++;
            var result = _results.Count > 0 ? _results.Dequeue() : SyncTransportResult.Success();
            return Task.FromResult(result);
        }
    }
}
