namespace SfaApp.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int TodaysVisitCount { get; set; }
    public int TodaysOrderCount { get; set; }
}