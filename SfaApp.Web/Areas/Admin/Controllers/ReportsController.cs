using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.ViewModels.Admin;
using System.Globalization;
using System.Text;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,TSI")]
public class ReportsController : Controller
{
    private static readonly TimeZoneInfo BusinessTimeZone = ResolveBusinessTimeZone();

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ApplicationDbContext dbContext, ILogger<ReportsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index(DateOnly? fromDate, DateOnly? toDate, string? repUserId, int? routeId)
    {
        try
        {
            var model = await BuildReportModelAsync(fromDate, toDate, repUserId, routeId);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to load report metrics");
            TempData["ErrorMessage"] = "Unable to load report metrics.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while loading report metrics");
            TempData["ErrorMessage"] = "Unable to load report metrics.";
        }

        return View(new ReportsIndexViewModel());
    }

    public async Task<IActionResult> Export(string format = "csv", DateOnly? fromDate = null, DateOnly? toDate = null, string? repUserId = null, int? routeId = null)
    {
        try
        {
            var model = await BuildReportModelAsync(fromDate, toDate, repUserId, routeId);
            if (format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ExportXlsx(model);
            }

            return ExportCsv(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to export report");
            TempData["ErrorMessage"] = "Unable to export report.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate, repUserId, routeId });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while exporting report");
            TempData["ErrorMessage"] = "Unable to export report.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate, repUserId, routeId });
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while exporting report");
            TempData["ErrorMessage"] = "Unable to export report.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate, repUserId, routeId });
        }
    }

    private async Task<ReportsIndexViewModel> BuildReportModelAsync(DateOnly? fromDate, DateOnly? toDate, string? repUserId, int? routeId)
    {
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZone));
        var effectiveFromDate = fromDate ?? todayLocal;
        var effectiveToDate = toDate ?? todayLocal;

        if (effectiveFromDate > effectiveToDate)
        {
            throw new InvalidOperationException("From date must be less than or equal to to date.");
        }

        var utcStart = ToUtcStart(effectiveFromDate);
        var utcEndExclusive = ToUtcStart(effectiveToDate.AddDays(1));

        var model = new ReportsIndexViewModel
        {
            FromDate = effectiveFromDate,
            ToDate = effectiveToDate,
            RepUserId = repUserId,
            RouteId = routeId
        };

        var salesRepRoleId = await _dbContext.Roles
            .Where(x => x.Name == "SalesRep")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(salesRepRoleId))
        {
            var repLookup = await (from user in _dbContext.Users
                                   join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                                   where userRole.RoleId == salesRepRoleId && user.IsActive
                                   orderby user.FullName, user.Email
                                   select new LookupItemViewModel
                                   {
                                       Value = user.Id,
                                       Text = $"{(user.FullName ?? user.Email ?? user.UserName ?? user.Id)} ({(user.Email ?? user.UserName ?? "-")})"
                                   })
                .Distinct()
                .ToListAsync();

            model.SalesReps = repLookup;
        }

        model.Routes = await _dbContext.SalesRoutes
            .Where(x => x.IsActive)
            .OrderBy(x => x.RouteName)
            .Select(x => new LookupItemViewModel
            {
                Value = x.Id.ToString(CultureInfo.InvariantCulture),
                Text = $"{x.RouteName} ({x.RouteCode})"
            })
            .ToListAsync();

        List<int>? repRouteIds = null;
        if (!string.IsNullOrWhiteSpace(repUserId))
        {
            repRouteIds = await _dbContext.RouteAssignments
                .Where(x => x.RepUserId == repUserId && x.IsActive)
                .Select(x => x.RouteId)
                .Distinct()
                .ToListAsync();
        }

        var ordersQuery = _dbContext.SalesOrders
            .AsNoTracking()
            .Include(x => x.Route)
            .Include(x => x.Customer)
            .Where(x => x.OrderDateUtc >= utcStart && x.OrderDateUtc < utcEndExclusive);

        if (!string.IsNullOrWhiteSpace(repUserId))
        {
            ordersQuery = ordersQuery.Where(x => x.RepUserId == repUserId);
        }

        if (routeId.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.RouteId == routeId.Value);
        }

        var visitsQuery = _dbContext.Visits
            .AsNoTracking()
            .Where(x => x.VisitDate >= effectiveFromDate && x.VisitDate <= effectiveToDate);

        if (!string.IsNullOrWhiteSpace(repUserId))
        {
            visitsQuery = visitsQuery.Where(x => x.RepUserId == repUserId);
        }

        if (routeId.HasValue)
        {
            visitsQuery = visitsQuery.Where(x => x.RouteId == routeId.Value);
        }

        var daySessionQuery = _dbContext.DaySessions
            .AsNoTracking()
            .Where(x => x.BusinessDate >= effectiveFromDate && x.BusinessDate <= effectiveToDate);

        if (!string.IsNullOrWhiteSpace(repUserId))
        {
            daySessionQuery = daySessionQuery.Where(x => x.RepUserId == repUserId);
        }

        if (routeId.HasValue)
        {
            daySessionQuery = daySessionQuery.Where(x => x.SelectedRouteId == routeId.Value);
        }

        var customersQuery = _dbContext.Customers
            .AsNoTracking()
            .Where(x => x.IsActive && (!x.Latitude.HasValue || !x.Longitude.HasValue));

        if (routeId.HasValue)
        {
            customersQuery = customersQuery.Where(x => x.RouteId == routeId.Value);
        }

        if (repRouteIds is not null)
        {
            customersQuery = customersQuery.Where(x => repRouteIds.Contains(x.RouteId));
        }

        var todayLocalStartUtc = ToUtcStart(todayLocal);
        var todayLocalEndUtc = ToUtcStart(todayLocal.AddDays(1));

        var todayOrdersQuery = _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.OrderDateUtc >= todayLocalStartUtc && x.OrderDateUtc < todayLocalEndUtc);

        if (!string.IsNullOrWhiteSpace(repUserId))
        {
            todayOrdersQuery = todayOrdersQuery.Where(x => x.RepUserId == repUserId);
        }

        if (routeId.HasValue)
        {
            todayOrdersQuery = todayOrdersQuery.Where(x => x.RouteId == routeId.Value);
        }

        model.CustomersWithoutCoordinates = await customersQuery.CountAsync();
        model.PendingDayEndCount = await daySessionQuery.CountAsync(x => x.Status == DaySessionStatus.Started);
        model.TotalOrders = await ordersQuery.CountAsync();
        model.DeliveredOrders = await ordersQuery.CountAsync(x => x.Status == OrderStatus.Delivered);
        model.TodayOrders = await todayOrdersQuery.CountAsync();
        model.SkippedVisits = await visitsQuery.CountAsync(x => x.VisitStatus == VisitStatus.Skipped);
        model.OutsideGeoToleranceVisits = await visitsQuery.CountAsync(x => x.WithinToleranceFlag == false);

        var pendingSyncItems = await _dbContext.SyncQueueItems
            .AsNoTracking()
            .Where(x => x.SyncStatus != SyncStatus.Synced && x.CreatedAtUtc >= utcStart && x.CreatedAtUtc < utcEndExclusive)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(repUserId) || routeId.HasValue)
        {
            var syncClientUuids = pendingSyncItems.Select(x => x.EntityClientUuid).Distinct().ToList();

            var daySessionFilter = await _dbContext.DaySessions
                .AsNoTracking()
                .Where(x => syncClientUuids.Contains(x.ClientGeneratedUuid))
                .Select(x => new { x.ClientGeneratedUuid, x.RepUserId, x.SelectedRouteId })
                .ToListAsync();

            var visitFilter = await _dbContext.Visits
                .AsNoTracking()
                .Where(x => syncClientUuids.Contains(x.ClientGeneratedUuid))
                .Select(x => new { x.ClientGeneratedUuid, x.RepUserId, x.RouteId })
                .ToListAsync();

            var orderFilter = await _dbContext.SalesOrders
                .AsNoTracking()
                .Where(x => syncClientUuids.Contains(x.ClientGeneratedUuid))
                .Select(x => new { x.ClientGeneratedUuid, x.RepUserId, x.RouteId })
                .ToListAsync();

            var daySessionMap = daySessionFilter.ToDictionary(x => x.ClientGeneratedUuid, StringComparer.OrdinalIgnoreCase);
            var visitMap = visitFilter.ToDictionary(x => x.ClientGeneratedUuid, StringComparer.OrdinalIgnoreCase);
            var orderMap = orderFilter.ToDictionary(x => x.ClientGeneratedUuid, StringComparer.OrdinalIgnoreCase);

            pendingSyncItems = pendingSyncItems.Where(item =>
            {
                if (item.EntityType == "DaySession" && daySessionMap.TryGetValue(item.EntityClientUuid, out var daySessionItem))
                {
                    return MatchesSyncFilter(repUserId, routeId, daySessionItem.RepUserId, daySessionItem.SelectedRouteId);
                }

                if (item.EntityType == "Visit" && visitMap.TryGetValue(item.EntityClientUuid, out var visitItem))
                {
                    return MatchesSyncFilter(repUserId, routeId, visitItem.RepUserId, visitItem.RouteId);
                }

                if (item.EntityType == "SalesOrder" && orderMap.TryGetValue(item.EntityClientUuid, out var orderItem))
                {
                    return MatchesSyncFilter(repUserId, routeId, orderItem.RepUserId, orderItem.RouteId);
                }

                return false;
            }).ToList();
        }

        model.PendingSyncItems = pendingSyncItems.Count;

        var orders = await ordersQuery
            .OrderByDescending(x => x.OrderDateUtc)
            .Take(500)
            .ToListAsync();

        var repIds = orders
            .Where(x => !string.IsNullOrWhiteSpace(x.RepUserId))
            .Select(x => x.RepUserId!)
            .Distinct()
            .ToList();

        var repNameMap = await _dbContext.Users
            .AsNoTracking()
            .Where(x => repIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => x.FullName ?? x.Email ?? x.UserName ?? x.Id,
                cancellationToken: default);

        model.Orders = orders
            .Select(x => new ReportOrderRowViewModel
            {
                OrderNumber = x.OrderNumber,
                OrderDateUtc = x.OrderDateUtc,
                RepName = !string.IsNullOrWhiteSpace(x.RepUserId) && repNameMap.TryGetValue(x.RepUserId!, out var repName)
                    ? repName
                    : "-",
                RouteName = x.Route?.RouteName ?? "-",
                CustomerName = x.Customer?.Name ?? "-",
                Status = x.Status.ToString(),
                TotalAmount = x.TotalAmount
            })
            .ToList();

        return model;
    }

    private FileContentResult ExportCsv(ReportsIndexViewModel model)
    {
        var builder = new StringBuilder();
        builder.AppendLine("OrderNumber,OrderDateUtc,Rep,Route,Customer,Status,TotalAmount");
        foreach (var row in model.Orders)
        {
            builder.AppendLine(string.Join(",",
                EscapeCsv(row.OrderNumber),
                EscapeCsv(row.OrderDateUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                EscapeCsv(row.RepName),
                EscapeCsv(row.RouteName),
                EscapeCsv(row.CustomerName),
                EscapeCsv(row.Status),
                row.TotalAmount.ToString(CultureInfo.InvariantCulture)));
        }

        var content = Encoding.UTF8.GetBytes(builder.ToString());
        return File(content, "text/csv", $"report_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private FileContentResult ExportXlsx(ReportsIndexViewModel model)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        worksheet.Cell(1, 1).Value = "Order Number";
        worksheet.Cell(1, 2).Value = "Order Date UTC";
        worksheet.Cell(1, 3).Value = "Rep";
        worksheet.Cell(1, 4).Value = "Route";
        worksheet.Cell(1, 5).Value = "Customer";
        worksheet.Cell(1, 6).Value = "Status";
        worksheet.Cell(1, 7).Value = "Total Amount";

        var rowNumber = 2;
        foreach (var row in model.Orders)
        {
            worksheet.Cell(rowNumber, 1).Value = row.OrderNumber;
            worksheet.Cell(rowNumber, 2).Value = row.OrderDateUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            worksheet.Cell(rowNumber, 3).Value = row.RepName;
            worksheet.Cell(rowNumber, 4).Value = row.RouteName;
            worksheet.Cell(rowNumber, 5).Value = row.CustomerName;
            worksheet.Cell(rowNumber, 6).Value = row.Status;
            worksheet.Cell(rowNumber, 7).Value = row.TotalAmount;
            rowNumber++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    private static DateTime ToUtcStart(DateOnly localDate)
    {
        var localDateTime = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, BusinessTimeZone);
    }

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
    }

    private static bool MatchesSyncFilter(string? repUserId, int? routeId, string? itemRepUserId, int? itemRouteId)
    {
        if (!string.IsNullOrWhiteSpace(repUserId) && !string.Equals(repUserId, itemRepUserId, StringComparison.Ordinal))
        {
            return false;
        }

        if (routeId.HasValue && itemRouteId != routeId.Value)
        {
            return false;
        }

        return true;
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}