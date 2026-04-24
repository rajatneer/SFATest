using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels;

public class CreateVisitViewModel
{
    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Visit Date")]
    public DateOnly VisitDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [StringLength(200)]
    public string? Outcome { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}