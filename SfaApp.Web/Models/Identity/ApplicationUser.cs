using Microsoft.AspNetCore.Identity;

namespace SfaApp.Web.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DistributorId { get; set; }
}