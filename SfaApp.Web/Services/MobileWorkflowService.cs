using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.ViewModels.Mobile;
using System.Text.Json;

namespace SfaApp.Web.Services;

public class MobileWorkflowService : IMobileWorkflowService
{
    private const decimal DefaultGeoToleranceMeters = 200;
    private const string DaySessionEntityType = "DaySession";
    private const string VisitEntityType = "Visit";
    private const string SalesOrderEntityType = "SalesOrder";

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MobileWorkflowService> _logger;

    public MobileWorkflowService(ApplicationDbContext dbContext, ILogger<MobileWorkflowService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DaySession?> GetActiveSessionAsync(string repUserId)
    {
        try
        {
            return await _dbContext.DaySessions
                .Include(x => x.SelectedRoute)
                .FirstOrDefaultAsync(x => x.RepUserId == repUserId && x.Status == DaySessionStatus.Started);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read active day session for rep {RepUserId}", repUserId);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while reading active day session for rep {RepUserId}", repUserId);
            throw;
        }
    }

    public async Task<List<SalesRoute>> GetAssignedRoutesAsync(string repUserId)
    {
        try
        {
            return await _dbContext.RouteAssignments
                .Where(x => x.RepUserId == repUserId && x.IsActive)
                .Include(x => x.Route)
                    .ThenInclude(x => x!.Territory)
                .Include(x => x.Route)
                    .ThenInclude(x => x!.Distributor)
                .Select(x => x.Route!)
                .OrderBy(x => x.RouteName)
                .ToListAsync();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read assigned routes for rep {RepUserId}", repUserId);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while reading assigned routes for rep {RepUserId}", repUserId);
            throw;
        }
    }

    public async Task<List<Customer>> GetCustomersForSelectedRouteAsync(string repUserId)
    {
        var session = await EnsureActiveSessionAsync(repUserId);
        if (session.SelectedRouteId is null)
        {
            return [];
        }

        try
        {
            var customers = await _dbContext.Customers
                .Where(x => x.RouteId == session.SelectedRouteId && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return customers;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read customers for rep {RepUserId}", repUserId);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while reading customers for rep {RepUserId}", repUserId);
            throw;
        }
    }

    public async Task<DaySession> StartDayAsync(string repUserId, decimal startLat, decimal startLong)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var activeSession = await _dbContext.DaySessions
                .FirstOrDefaultAsync(x => x.RepUserId == repUserId && x.Status == DaySessionStatus.Started);

            if (activeSession is not null)
            {
                return activeSession;
            }

            var daySession = new DaySession
            {
                RepUserId = repUserId,
                BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow),
                StartDayTimestampUtc = DateTime.UtcNow,
                StartDayLat = startLat,
                StartDayLong = startLong,
                Status = DaySessionStatus.Started,
                SyncStatus = SyncStatus.Pending
            };

            _dbContext.DaySessions.Add(daySession);
            await QueueSyncItemAsync(
                DaySessionEntityType,
                daySession.ClientGeneratedUuid,
                new
                {
                    daySession.RepUserId,
                    daySession.BusinessDate,
                    daySession.StartDayTimestampUtc,
                    daySession.StartDayLat,
                    daySession.StartDayLong,
                    daySession.Status
                },
                cancellationToken: default);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return daySession;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error while starting day for rep {RepUserId}", repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid operation while starting day for rep {RepUserId}", repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing start day for rep {RepUserId}", repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task SelectRouteAsync(string repUserId, int routeId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = await EnsureActiveSessionAsync(repUserId);

            var isAssigned = await _dbContext.RouteAssignments.AnyAsync(x =>
                x.RepUserId == repUserId &&
                x.RouteId == routeId &&
                x.IsActive);

            if (!isAssigned)
            {
                throw new InvalidOperationException("Route is not assigned to this sales rep.");
            }

            session.SelectedRouteId = routeId;
            session.SyncStatus = SyncStatus.Pending;

            await QueueSyncItemAsync(
                DaySessionEntityType,
                session.ClientGeneratedUuid,
                new
                {
                    session.RepUserId,
                    session.BusinessDate,
                    session.StartDayTimestampUtc,
                    session.EndDayTimestampUtc,
                    session.SelectedRouteId,
                    session.Status
                },
                cancellationToken: default);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error while selecting route {RouteId} for rep {RepUserId}", routeId, repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid operation while selecting route {RouteId} for rep {RepUserId}", routeId, repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing selected route {RouteId} for rep {RepUserId}", routeId, repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<Visit> CheckInAsync(string repUserId, CheckInViewModel model)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = await EnsureActiveSessionAsync(repUserId);
            if (session.SelectedRouteId is null)
            {
                throw new InvalidOperationException("Please select route before check-in.");
            }

            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == model.CustomerId && x.IsActive);
            if (customer is null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            if (customer.RouteId != session.SelectedRouteId)
            {
                throw new InvalidOperationException("Customer is outside selected route.");
            }

            decimal? distanceMeters = null;
            bool? withinTolerance = null;
            var capturedDuringVisit = false;

            if (customer.Latitude.HasValue && customer.Longitude.HasValue)
            {
                distanceMeters = CalculateDistanceMeters(
                    customer.Latitude.Value,
                    customer.Longitude.Value,
                    model.DeviceLat,
                    model.DeviceLong);

                withinTolerance = distanceMeters <= DefaultGeoToleranceMeters;
            }
            else if (model.CaptureCustomerCoordinates)
            {
                customer.Latitude = model.DeviceLat;
                customer.Longitude = model.DeviceLong;
                customer.CoordinateCaptureSource = "rep_capture";
                customer.CoordinateCaptureTimestamp = DateTime.UtcNow;
                customer.UpdatedAtUtc = DateTime.UtcNow;
                capturedDuringVisit = true;
            }

            var visit = new Visit
            {
                RepUserId = repUserId,
                DaySessionId = session.Id,
                RouteId = session.SelectedRouteId,
                CustomerId = customer.Id,
                VisitDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CheckinTimestampUtc = DateTime.UtcNow,
                CheckinLat = model.DeviceLat,
                CheckinLong = model.DeviceLong,
                CustomerRefLat = customer.Latitude,
                CustomerRefLong = customer.Longitude,
                GeoDistanceMeters = distanceMeters,
                WithinToleranceFlag = withinTolerance,
                CoordinateCapturedDuringVisitFlag = capturedDuringVisit,
                VisitStatus = VisitStatus.Completed,
                Outcome = "Checked In",
                CreatedByUserId = repUserId,
                CreatedAtUtc = DateTime.UtcNow,
                SyncStatus = SyncStatus.Pending
            };

            _dbContext.Visits.Add(visit);
            await QueueSyncItemAsync(
                VisitEntityType,
                visit.ClientGeneratedUuid,
                new
                {
                    visit.RepUserId,
                    visit.DaySessionId,
                    visit.RouteId,
                    visit.CustomerId,
                    visit.VisitDate,
                    visit.CheckinTimestampUtc,
                    visit.CheckinLat,
                    visit.CheckinLong,
                    visit.VisitStatus,
                    visit.Outcome
                },
                cancellationToken: default);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return visit;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error during check-in for rep {RepUserId}", repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid check-in operation for rep {RepUserId}", repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing check-in sync for rep {RepUserId}", repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task CheckoutVisitAsync(string repUserId, CheckoutViewModel model)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var visit = await _dbContext.Visits.FirstOrDefaultAsync(x => x.Id == model.VisitId && x.RepUserId == repUserId);
            if (visit is null)
            {
                throw new InvalidOperationException("Visit not found.");
            }

            visit.CheckoutTimestampUtc = DateTime.UtcNow;
            visit.CheckoutLat = model.CheckoutLat;
            visit.CheckoutLong = model.CheckoutLong;
            visit.Notes = model.VisitNotes;
            visit.VisitStatus = model.VisitStatus;
            visit.Outcome = model.VisitStatus.ToString();
            visit.SyncStatus = SyncStatus.Pending;

            await QueueSyncItemAsync(
                VisitEntityType,
                visit.ClientGeneratedUuid,
                new
                {
                    visit.RepUserId,
                    visit.DaySessionId,
                    visit.RouteId,
                    visit.CustomerId,
                    visit.VisitDate,
                    visit.CheckinTimestampUtc,
                    visit.CheckoutTimestampUtc,
                    visit.CheckoutLat,
                    visit.CheckoutLong,
                    visit.VisitStatus,
                    visit.Outcome,
                    visit.Notes
                },
                cancellationToken: default);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error during checkout for rep {RepUserId}", repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid checkout operation for rep {RepUserId}", repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing checkout sync for rep {RepUserId}", repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<SalesOrder> CreateOrderAsync(string repUserId, CreateMobileOrderViewModel model)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = await EnsureActiveSessionAsync(repUserId);
            if (session.SelectedRouteId is null)
            {
                throw new InvalidOperationException("Route not selected.");
            }

            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == model.CustomerId && x.IsActive);
            if (customer is null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            if (customer.RouteId != session.SelectedRouteId)
            {
                throw new InvalidOperationException("Customer is outside selected route.");
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == model.ProductId && x.IsActive);
            if (product is null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            var route = await _dbContext.SalesRoutes.FirstOrDefaultAsync(x => x.Id == session.SelectedRouteId);
            if (route is null)
            {
                throw new InvalidOperationException("Selected route is invalid.");
            }

            var lineTotal = model.Quantity * product.UnitPrice;
            var order = new SalesOrder
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                OrderDateUtc = DateTime.UtcNow,
                DaySessionId = session.Id,
                RouteId = route.Id,
                DistributorId = route.DistributorId,
                RepUserId = repUserId,
                CustomerId = customer.Id,
                VisitId = model.VisitId,
                Status = OrderStatus.Created,
                TotalAmount = lineTotal,
                GrossAmount = lineTotal,
                NetAmount = lineTotal,
                Source = "pwa",
                SyncStatus = SyncStatus.Pending,
                Lines =
                [
                    new SalesOrderLine
                    {
                        ProductId = product.Id,
                        Quantity = model.Quantity,
                        UnitPrice = product.UnitPrice,
                        LineTotal = lineTotal
                    }
                ]
            };

            _dbContext.SalesOrders.Add(order);
            await QueueSyncItemAsync(
                SalesOrderEntityType,
                order.ClientGeneratedUuid,
                new
                {
                    order.OrderNumber,
                    order.OrderDateUtc,
                    order.RepUserId,
                    order.DaySessionId,
                    order.RouteId,
                    order.DistributorId,
                    order.CustomerId,
                    order.VisitId,
                    order.TotalAmount,
                    order.GrossAmount,
                    order.NetAmount,
                    order.Status,
                    order.Source,
                    ProductId = model.ProductId,
                    model.Quantity
                },
                cancellationToken: default);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error while creating order for rep {RepUserId}", repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid operation while creating order for rep {RepUserId}", repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing order sync for rep {RepUserId}", repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task EndDayAsync(string repUserId, decimal endLat, decimal endLong)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = await EnsureActiveSessionAsync(repUserId);
            session.EndDayTimestampUtc = DateTime.UtcNow;
            session.EndDayLat = endLat;
            session.EndDayLong = endLong;
            session.Status = DaySessionStatus.Ended;
            session.SyncStatus = SyncStatus.Pending;

            await QueueSyncItemAsync(
                DaySessionEntityType,
                session.ClientGeneratedUuid,
                new
                {
                    session.RepUserId,
                    session.BusinessDate,
                    session.StartDayTimestampUtc,
                    session.EndDayTimestampUtc,
                    session.StartDayLat,
                    session.StartDayLong,
                    session.EndDayLat,
                    session.EndDayLong,
                    session.SelectedRouteId,
                    session.Status
                },
                cancellationToken: default);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Database error while ending day for rep {RepUserId}", repUserId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Invalid operation while ending day for rep {RepUserId}", repUserId);
            throw;
        }
        catch (JsonException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "JSON error while queueing end day sync for rep {RepUserId}", repUserId);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<List<SyncQueueItem>> GetPendingQueueAsync()
    {
        try
        {
            return await _dbContext.SyncQueueItems
                .Where(x => x.SyncStatus != SyncStatus.Synced)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read sync queue");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while reading sync queue");
            throw;
        }
    }

    private async Task QueueSyncItemAsync(string entityType, string entityClientUuid, object payload, CancellationToken cancellationToken)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        var existingItem = await _dbContext.SyncQueueItems
            .FirstOrDefaultAsync(x => x.EntityClientUuid == entityClientUuid, cancellationToken);

        if (existingItem is null)
        {
            _dbContext.SyncQueueItems.Add(new SyncQueueItem
            {
                EntityType = entityType,
                EntityClientUuid = entityClientUuid,
                PayloadJson = payloadJson,
                CreatedAtUtc = DateTime.UtcNow,
                RetryCount = 0,
                LastRetryAtUtc = null,
                SyncStatus = SyncStatus.Pending,
                LastErrorMessage = null
            });
            return;
        }

        existingItem.EntityType = entityType;
        existingItem.PayloadJson = payloadJson;
        existingItem.RetryCount = 0;
        existingItem.LastRetryAtUtc = null;
        existingItem.SyncStatus = SyncStatus.Pending;
        existingItem.LastErrorMessage = null;
    }

    private async Task<DaySession> EnsureActiveSessionAsync(string repUserId)
    {
        var session = await _dbContext.DaySessions
            .FirstOrDefaultAsync(x => x.RepUserId == repUserId && x.Status == DaySessionStatus.Started);

        if (session is null)
        {
            throw new InvalidOperationException("Start day before performing this action.");
        }

        return session;
    }

    private static decimal CalculateDistanceMeters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var radius = 6371000d;
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(radius * c);
    }

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}
