using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Tests.Infrastructure;

namespace SfaApp.Web.Tests;

[Trait("Category", "Integration")]
public class ReportsIntegrationTests
{
    [Fact]
    public async Task ExportCsv_AppliesDateRepAndRouteFilters()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var seeded = await SeedReportDataAsync(factory);
        var filterDate = seeded.FilterDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var response = await client.GetAsync($"/admin/reports/export?format=csv&fromDate={filterDate}&toDate={filterDate}&repUserId={seeded.RepUserId}&routeId={seeded.RouteId}");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains("ORD-MATCH", csv);
        Assert.DoesNotContain("ORD-ROUTE-MISS", csv);
        Assert.DoesNotContain("ORD-REP-MISS", csv);
        Assert.DoesNotContain("ORD-DATE-MISS", csv);
    }

    [Fact]
    public async Task ExportXlsx_ReturnsWorkbookForFilteredData()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var seeded = await SeedReportDataAsync(factory);
        var filterDate = seeded.FilterDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var response = await client.GetAsync($"/admin/reports/export?format=xlsx&fromDate={filterDate}&toDate={filterDate}&repUserId={seeded.RepUserId}&routeId={seeded.RouteId}");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType?.MediaType);

        var payload = await response.Content.ReadAsByteArrayAsync();
        Assert.True(payload.Length > 200);
        Assert.Equal((byte)'P', payload[0]);
        Assert.Equal((byte)'K', payload[1]);
    }

    private static async Task<(DateOnly FilterDate, string RepUserId, int RouteId)> SeedReportDataAsync(IntegrationWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var territory = new Territory { TerritoryCode = "T-001", TerritoryName = "Territory 1" };
        var distributor = new Distributor { DistributorCode = "D-001", DistributorName = "Distributor 1" };
        dbContext.AddRange(territory, distributor);
        await dbContext.SaveChangesAsync();

        var routeMatch = new SalesRoute
        {
            RouteCode = "R-MATCH",
            RouteName = "Route Match",
            TerritoryId = territory.Id,
            DistributorId = distributor.Id,
            IsActive = true
        };

        var routeOther = new SalesRoute
        {
            RouteCode = "R-OTHER",
            RouteName = "Route Other",
            TerritoryId = territory.Id,
            DistributorId = distributor.Id,
            IsActive = true
        };

        dbContext.SalesRoutes.AddRange(routeMatch, routeOther);
        await dbContext.SaveChangesAsync();

        var customerMatch = new Customer
        {
            CustomerCode = "C-001",
            Name = "Customer Match",
            OutletCode = "OUT-001",
            RouteId = routeMatch.Id,
            TerritoryId = territory.Id,
            DistributorId = distributor.Id,
            IsActive = true
        };

        var customerOther = new Customer
        {
            CustomerCode = "C-002",
            Name = "Customer Other",
            OutletCode = "OUT-002",
            RouteId = routeOther.Id,
            TerritoryId = territory.Id,
            DistributorId = distributor.Id,
            IsActive = true
        };

        dbContext.Customers.AddRange(customerMatch, customerOther);

        var repUserId = "rep-filter";
        var repUserOtherId = "rep-other";

        dbContext.Users.AddRange(
            new ApplicationUser
            {
                Id = repUserId,
                UserName = "rep.filter@sfa.local",
                NormalizedUserName = "REP.FILTER@SFA.LOCAL",
                Email = "rep.filter@sfa.local",
                NormalizedEmail = "REP.FILTER@SFA.LOCAL",
                FullName = "Rep Filter",
                SecurityStamp = Guid.NewGuid().ToString("N")
            },
            new ApplicationUser
            {
                Id = repUserOtherId,
                UserName = "rep.other@sfa.local",
                NormalizedUserName = "REP.OTHER@SFA.LOCAL",
                Email = "rep.other@sfa.local",
                NormalizedEmail = "REP.OTHER@SFA.LOCAL",
                FullName = "Rep Other",
                SecurityStamp = Guid.NewGuid().ToString("N")
            });

        await dbContext.SaveChangesAsync();

        var filterDate = GetBusinessDateToday();

        dbContext.SalesOrders.AddRange(
            new SalesOrder
            {
                OrderNumber = "ORD-MATCH",
                CustomerId = customerMatch.Id,
                RouteId = routeMatch.Id,
                DistributorId = distributor.Id,
                RepUserId = repUserId,
                OrderDateUtc = ToUtcBusinessDate(filterDate),
                Status = OrderStatus.Delivered,
                TotalAmount = 120m,
                GrossAmount = 120m,
                NetAmount = 120m
            },
            new SalesOrder
            {
                OrderNumber = "ORD-ROUTE-MISS",
                CustomerId = customerOther.Id,
                RouteId = routeOther.Id,
                DistributorId = distributor.Id,
                RepUserId = repUserId,
                OrderDateUtc = ToUtcBusinessDate(filterDate),
                Status = OrderStatus.Delivered,
                TotalAmount = 140m,
                GrossAmount = 140m,
                NetAmount = 140m
            },
            new SalesOrder
            {
                OrderNumber = "ORD-REP-MISS",
                CustomerId = customerMatch.Id,
                RouteId = routeMatch.Id,
                DistributorId = distributor.Id,
                RepUserId = repUserOtherId,
                OrderDateUtc = ToUtcBusinessDate(filterDate),
                Status = OrderStatus.Delivered,
                TotalAmount = 160m,
                GrossAmount = 160m,
                NetAmount = 160m
            },
            new SalesOrder
            {
                OrderNumber = "ORD-DATE-MISS",
                CustomerId = customerMatch.Id,
                RouteId = routeMatch.Id,
                DistributorId = distributor.Id,
                RepUserId = repUserId,
                OrderDateUtc = ToUtcBusinessDate(filterDate.AddDays(-1)),
                Status = OrderStatus.Delivered,
                TotalAmount = 180m,
                GrossAmount = 180m,
                NetAmount = 180m
            });

        await dbContext.SaveChangesAsync();

        return (filterDate, repUserId, routeMatch.Id);
    }

    private static DateOnly GetBusinessDateToday()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ResolveBusinessTimeZone()));
    }

    private static DateTime ToUtcBusinessDate(DateOnly localDate)
    {
        var localDateTime = localDate.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, ResolveBusinessTimeZone());
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
}
