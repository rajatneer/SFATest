using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string OutletCode { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? AlternateMobileNumber { get; set; }

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? Locality { get; set; }

    [StringLength(80)]
    public string? City { get; set; }

    [StringLength(80)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? Pincode { get; set; }

    [StringLength(30)]
    public string? GstNumber { get; set; }

    [StringLength(50)]
    public string? OutletType { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [StringLength(30)]
    public string? CoordinateCaptureSource { get; set; }

    public DateTime? CoordinateCaptureTimestamp { get; set; }

    public int? RouteId { get; set; }
    public SalesRoute? Route { get; set; }

    public int? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    public int? DistributorId { get; set; }
    public Distributor? Distributor { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}