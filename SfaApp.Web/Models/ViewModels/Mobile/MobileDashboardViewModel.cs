using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class MobileDashboardViewModel
{
    public DaySession? ActiveSession { get; set; }
    public string? ActiveRouteName { get; set; }
    public int PendingSyncCount { get; set; }
    public int VisitedCustomers { get; set; }
    public int OrdersCreated { get; set; }
}