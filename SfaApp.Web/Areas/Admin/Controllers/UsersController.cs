using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Models.ViewModels.Admin;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.Roles = _roleManager.Roles.OrderBy(x => x.Name).ToList();
        return View(_userManager.Users.OrderBy(x => x.Email).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide valid user details.";
            return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
        }

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null)
            {
                TempData["ErrorMessage"] = "User already exists.";
                return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", createResult.Errors.Select(x => x.Description));
                return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
            }

            await _userManager.AddToRoleAsync(user, model.RoleName);
            TempData["SuccessMessage"] = "User created.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "User creation failed");
            TempData["ErrorMessage"] = "Unable to create user.";
        }

        return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
    }
}