using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels;

public class CreateSalesOrderViewModel
{
    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Range(1, 999999)]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? Notes { get; set; }
}