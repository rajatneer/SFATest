using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class Territory
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string TerritoryCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string TerritoryName { get; set; } = string.Empty;

    public string? TsiUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SalesRoute> Routes { get; set; } = new List<SalesRoute>();
}