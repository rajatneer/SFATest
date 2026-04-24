using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class SalesRoute
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string RouteCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string RouteName { get; set; } = string.Empty;

    public int TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    public int DistributorId { get; set; }
    public Distributor? Distributor { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<RouteAssignment> Assignments { get; set; } = new List<RouteAssignment>();
}