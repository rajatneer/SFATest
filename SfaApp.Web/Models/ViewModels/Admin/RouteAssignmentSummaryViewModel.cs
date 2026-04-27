namespace SfaApp.Web.Models.ViewModels.Admin;

public class RouteAssignmentSummaryViewModel
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string RepName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
}
