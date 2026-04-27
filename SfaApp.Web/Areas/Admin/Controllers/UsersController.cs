using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Models.ViewModels.Admin;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(x => x.Email)
            .ToListAsync();

        var userIds = users.Select(x => x.Id).ToList();
        var userRoles = await (from userRole in _dbContext.UserRoles
                               join role in _dbContext.Roles on userRole.RoleId equals role.Id
                               where userIds.Contains(userRole.UserId)
                               select new { userRole.UserId, role.Name })
            .ToListAsync();

        var userRoleLookup = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                group => group.Key,
                group => string.Join(", ", group.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x))));

        var managerUserIds = users
            .Where(x => !string.IsNullOrWhiteSpace(x.ManagerUserId))
            .Select(x => x.ManagerUserId!)
            .Distinct()
            .ToList();

        var managerLookup = managerUserIds.Count == 0
            ? new Dictionary<string, string>()
            : await _dbContext.Users
                .Where(x => managerUserIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.FullName ?? x.Email ?? x.UserName ?? x.Id);

        var distributorLookup = await _dbContext.Distributors
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Id,
                x => x.DistributorName);

        var tsiRoleId = await _dbContext.Roles
            .Where(x => x.Name == "TSI")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var tsiUsers = string.IsNullOrWhiteSpace(tsiRoleId)
            ? []
            : await (from user in _dbContext.Users
                     join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                     where userRole.RoleId == tsiRoleId && user.IsActive
                     orderby user.FullName, user.Email
                     select user)
                .Distinct()
                .ToListAsync();

        ViewBag.Roles = _roleManager.Roles.OrderBy(x => x.Name).ToList();
        ViewBag.UserRoleLookup = userRoleLookup;
        ViewBag.ManagerLookup = managerLookup;
        ViewBag.DistributorLookup = distributorLookup;
        ViewBag.TsiUsers = tsiUsers;
        ViewBag.Distributors = await _dbContext.Distributors
            .Where(x => x.IsActive)
            .OrderBy(x => x.DistributorName)
            .ToListAsync();

        return View(users);
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
            var normalizedRoleName = model.RoleName.Trim();

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser is not null)
            {
                TempData["ErrorMessage"] = "User already exists.";
                return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
            }

            if (string.Equals(normalizedRoleName, "SalesRep", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.ManagerUserId))
                {
                    TempData["ErrorMessage"] = "Sales rep must be mapped to a TSI.";
                    return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
                }

                var tsiRoleId = await _dbContext.Roles
                    .Where(x => x.Name == "TSI")
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(tsiRoleId))
                {
                    TempData["ErrorMessage"] = "TSI role is not configured.";
                    return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
                }

                var managerExists = await (from user in _dbContext.Users
                                           join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                                           where user.Id == model.ManagerUserId && user.IsActive && userRole.RoleId == tsiRoleId
                                           select user.Id)
                    .AnyAsync();

                if (!managerExists)
                {
                    TempData["ErrorMessage"] = "Select a valid active TSI manager for this sales rep.";
                    return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
                }
            }

            if (string.Equals(normalizedRoleName, "DistributorUser", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.DistributorId.HasValue || model.DistributorId.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Distributor user must be mapped to a distributor.";
                    return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
                }

                var distributorExists = await _dbContext.Distributors
                    .AnyAsync(x => x.Id == model.DistributorId.Value && x.IsActive);

                if (!distributorExists)
                {
                    TempData["ErrorMessage"] = "Select a valid active distributor.";
                    return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
                }
            }

            var appUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = true,
                ManagerUserId = string.Equals(normalizedRoleName, "SalesRep", StringComparison.OrdinalIgnoreCase)
                    ? model.ManagerUserId?.Trim()
                    : null,
                DistributorId = string.Equals(normalizedRoleName, "DistributorUser", StringComparison.OrdinalIgnoreCase)
                    ? model.DistributorId
                    : null
            };

            var createResult = await _userManager.CreateAsync(appUser, model.Password);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", createResult.Errors.Select(x => x.Description));
                return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
            }

            if (!await _roleManager.RoleExistsAsync(normalizedRoleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(normalizedRoleName));
            }

            await _userManager.AddToRoleAsync(appUser, normalizedRoleName);
            TempData["SuccessMessage"] = "User created.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "User creation failed");
            TempData["ErrorMessage"] = "Unable to create user.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "User creation failed");
            TempData["ErrorMessage"] = "Unable to create user.";
        }

        return RedirectToAction(nameof(Index), "Users", new { area = "Admin" });
    }
}