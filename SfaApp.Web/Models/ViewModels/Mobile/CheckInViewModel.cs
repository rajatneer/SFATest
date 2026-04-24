using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class CheckInViewModel
{
    [Required]
    public int CustomerId { get; set; }

    [Range(-90, 90)]
    public decimal DeviceLat { get; set; }

    [Range(-180, 180)]
    public decimal DeviceLong { get; set; }

    public bool CaptureCustomerCoordinates { get; set; }
}