using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels.Mobile;

public class MobileLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}