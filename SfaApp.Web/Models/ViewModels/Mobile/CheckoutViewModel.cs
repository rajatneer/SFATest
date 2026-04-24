using System.ComponentModel.DataAnnotations;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class CheckoutViewModel
{
    [Required]
    public int VisitId { get; set; }

    [Range(-90, 90)]
    public decimal CheckoutLat { get; set; }

    [Range(-180, 180)]
    public decimal CheckoutLong { get; set; }

    [StringLength(1000)]
    public string? VisitNotes { get; set; }

    public VisitStatus VisitStatus { get; set; } = VisitStatus.Completed;
}