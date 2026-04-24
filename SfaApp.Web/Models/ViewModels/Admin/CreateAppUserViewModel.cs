using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.ViewModels.Admin;

public class CreateAppUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = "SalesRep";
}