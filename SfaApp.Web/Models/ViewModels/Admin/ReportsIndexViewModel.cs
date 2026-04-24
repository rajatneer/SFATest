namespace SfaApp.Web.Models.ViewModels.Admin;

public class ReportsIndexViewModel
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? RepUserId { get; set; }
    public int? RouteId { get; set; }

    public int CustomersWithoutCoordinates { get; set; }
    public int PendingDayEndCount { get; set; }
    public int TotalOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int TodayOrders { get; set; }
    public int SkippedVisits { get; set; }
    public int OutsideGeoToleranceVisits { get; set; }
    public int PendingSyncItems { get; set; }

    public List<ReportOrderRowViewModel> Orders { get; set; } = [];
    public List<LookupItemViewModel> SalesReps { get; set; } = [];
    public List<LookupItemViewModel> Routes { get; set; } = [];
}

public class ReportOrderRowViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public string RepName { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class LookupItemViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
