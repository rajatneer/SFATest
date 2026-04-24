using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class Distributor
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string DistributorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string DistributorName { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    public string? MobileNumber { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SalesRoute> Routes { get; set; } = new List<SalesRoute>();
}