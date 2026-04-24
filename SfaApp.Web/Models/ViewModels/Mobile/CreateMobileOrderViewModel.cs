using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class CreateMobileOrderViewModel
{
    [Required]
    public int CustomerId { get; set; }

    public int? VisitId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(0.001, 999999)]
    public decimal Quantity { get; set; } = 1;
}