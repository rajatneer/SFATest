using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RolesController> _logger;

    public RolesController(RoleManager<IdentityRole> roleManager, ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(_roleManager.Roles.OrderBy(x => x.Name).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["ErrorMessage"] = "Role name is required.";
            return RedirectToAction(nameof(Index), "Roles", new { area = "Admin" });
        }

        try
        {
            var normalizedRoleName = roleName.Trim();
            if (await _roleManager.RoleExistsAsync(normalizedRoleName))
            {
                TempData["ErrorMessage"] = "Role already exists.";
                return RedirectToAction(nameof(Index), "Roles", new { area = "Admin" });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(normalizedRoleName));
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(x => x.Description));
                return RedirectToAction(nameof(Index), "Roles", new { area = "Admin" });
            }

            TempData["SuccessMessage"] = "Role added.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Role creation failed for role {RoleName}", roleName);
            TempData["ErrorMessage"] = "Unable to add role.";
        }

        return RedirectToAction(nameof(Index), "Roles", new { area = "Admin" });
    }
}