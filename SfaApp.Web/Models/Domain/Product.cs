using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Uom { get; set; }

    public decimal? Mrp { get; set; }

    [Range(0.01, 9999999)]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}