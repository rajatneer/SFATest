using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class CreateMobileOrderViewModel
{
    [Required]
    public int CustomerId { get; set; }

    public int? VisitId { get; set; }

    [StringLength(100)]
    public string? TimeZoneId { get; set; }

    [Range(-900, 900)]
    public int? UtcOffsetMinutes { get; set; }

    [MinLength(1, ErrorMessage = "Add at least one order line.")]
    public List<CreateMobileOrderLineViewModel> OrderLines { get; set; } =
    [
        new CreateMobileOrderLineViewModel()
    ];
}

public class CreateMobileOrderLineViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a product.")]
    public int ProductId { get; set; }

    [Range(0.01, 999999, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; } = 1;
}