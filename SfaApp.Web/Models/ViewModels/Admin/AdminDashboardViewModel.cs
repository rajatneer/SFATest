namespace SfaApp.Web.Models.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int UserCount { get; set; }
    public int TerritoryCount { get; set; }
    public int RouteCount { get; set; }
    public int CustomerCount { get; set; }
    public int UploadJobsPending { get; set; }
}