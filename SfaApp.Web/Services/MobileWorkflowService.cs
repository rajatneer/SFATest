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

    public async Task<DaySession> StartDayAsync(
        string repUserId,
        decimal startLat,
        decimal startLong,
        string? startTimeZoneId,
        int? startUtcOffsetMinutes)
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
                StartDayTimeZoneId = NormalizeTimeZoneId(startTimeZoneId),
                StartDayUtcOffsetMinutes = startUtcOffsetMinutes,
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
                    daySession.StartDayTimeZoneId,
                    daySession.StartDayUtcOffsetMinutes,
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
                    session.StartDayTimeZoneId,
                    session.StartDayUtcOffsetMinutes,
                    session.EndDayTimestampUtc,
                    session.EndDayTimeZoneId,
                    session.EndDayUtcOffsetMinutes,
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
                CheckinTimeZoneId = NormalizeTimeZoneId(model.TimeZoneId),
                CheckinUtcOffsetMinutes = model.UtcOffsetMinutes,
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
                    visit.CheckinTimeZoneId,
                    visit.CheckinUtcOffsetMinutes,
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
            visit.CheckoutTimeZoneId = NormalizeTimeZoneId(model.TimeZoneId);
            visit.CheckoutUtcOffsetMinutes = model.UtcOffsetMinutes;
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
                    visit.CheckoutTimeZoneId,
                    visit.CheckoutUtcOffsetMinutes,
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

            var orderLineSource = model.OrderLines ?? [];
            var orderLines = orderLineSource
                .Where(x => x.ProductId > 0 && x.Quantity > 0)
                .GroupBy(x => x.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(line => line.Quantity)
                })
                .ToList();

            if (orderLines.Count == 0)
            {
                throw new InvalidOperationException("At least one valid order line is required.");
            }

            var productIds = orderLines.Select(x => x.ProductId).ToList();
            var products = await _dbContext.Products
                .Where(x => productIds.Contains(x.Id) && x.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                throw new InvalidOperationException("One or more selected products are invalid or inactive.");
            }

            var productById = products.ToDictionary(x => x.Id);

            var route = await _dbContext.SalesRoutes.FirstOrDefaultAsync(x => x.Id == session.SelectedRouteId);
            if (route is null)
            {
                throw new InvalidOperationException("Selected route is invalid.");
            }

            var lineEntities = new List<SalesOrderLine>();
            foreach (var orderLine in orderLines)
            {
                if (!productById.TryGetValue(orderLine.ProductId, out var mappedProduct))
                {
                    throw new InvalidOperationException("One or more selected products are invalid or inactive.");
                }

                var currentLineTotal = orderLine.Quantity * mappedProduct.UnitPrice;
                lineEntities.Add(new SalesOrderLine
                {
                    ProductId = mappedProduct.Id,
                    Quantity = orderLine.Quantity,
                    UnitPrice = mappedProduct.UnitPrice,
                    LineTotal = currentLineTotal
                });
            }

            var orderTotal = lineEntities.Sum(x => x.LineTotal);
            var order = new SalesOrder
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                OrderDateUtc = DateTime.UtcNow,
                TimeZoneId = NormalizeTimeZoneId(model.TimeZoneId),
                UtcOffsetMinutes = model.UtcOffsetMinutes,
                DaySessionId = session.Id,
                RouteId = route.Id,
                DistributorId = route.DistributorId,
                RepUserId = repUserId,
                CustomerId = customer.Id,
                VisitId = model.VisitId,
                Status = OrderStatus.Created,
                TotalAmount = orderTotal,
                GrossAmount = orderTotal,
                NetAmount = orderTotal,
                Source = "pwa",
                SyncStatus = SyncStatus.Pending,
                Lines = lineEntities
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
                    order.TimeZoneId,
                    order.UtcOffsetMinutes,
                    Lines = order.Lines.Select(line => new
                    {
                        line.ProductId,
                        line.Quantity,
                        line.UnitPrice,
                        line.LineTotal
                    }).ToList()
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

    public async Task EndDayAsync(
        string repUserId,
        decimal endLat,
        decimal endLong,
        string? endTimeZoneId,
        int? endUtcOffsetMinutes)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = await EnsureActiveSessionAsync(repUserId);
            session.EndDayTimestampUtc = DateTime.UtcNow;
            session.EndDayLat = endLat;
            session.EndDayLong = endLong;
            session.EndDayTimeZoneId = NormalizeTimeZoneId(endTimeZoneId);
            session.EndDayUtcOffsetMinutes = endUtcOffsetMinutes;
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
                    session.StartDayTimeZoneId,
                    session.StartDayUtcOffsetMinutes,
                    session.EndDayLat,
                    session.EndDayLong,
                    session.EndDayTimeZoneId,
                    session.EndDayUtcOffsetMinutes,
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

    private static string? NormalizeTimeZoneId(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return null;
        }

        var trimmed = timeZoneId.Trim();
        return trimmed.Length > 100 ? trimmed[..100] : trimmed;
    }
}
