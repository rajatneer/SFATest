using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Models.ViewModels.Admin;

public class MasterSetupViewModel
{
    public List<Territory> Territories { get; set; } = [];
    public List<Distributor> Distributors { get; set; } = [];
    public List<SalesRoute> Routes { get; set; } = [];
    public List<LookupItemViewModel> SalesReps { get; set; } = [];
    public List<RouteAssignmentSummaryViewModel> ActiveRouteAssignments { get; set; } = [];
}