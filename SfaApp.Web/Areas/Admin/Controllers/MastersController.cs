using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.ViewModels.Admin;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MastersController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MastersController> _logger;

    public MastersController(ApplicationDbContext dbContext, ILogger<MastersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var model = new MasterSetupViewModel
        {
            Territories = await _dbContext.Territories.OrderBy(x => x.TerritoryName).ToListAsync(),
            Distributors = await _dbContext.Distributors.OrderBy(x => x.DistributorName).ToListAsync(),
            Routes = await _dbContext.SalesRoutes
                .Include(x => x.Territory)
                .Include(x => x.Distributor)
                .OrderBy(x => x.RouteName)
                .ToListAsync()
        };

        ViewBag.TerritoryOptions = new SelectList(model.Territories, nameof(Territory.Id), nameof(Territory.TerritoryName));
        ViewBag.DistributorOptions = new SelectList(model.Distributors, nameof(Distributor.Id), nameof(Distributor.DistributorName));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTerritory(string territoryCode, string territoryName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(territoryCode) || string.IsNullOrWhiteSpace(territoryName))
            {
                TempData["ErrorMessage"] = "Territory code and name are required.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var normalizedCode = territoryCode.Trim();
            var territoryExists = await _dbContext.Territories.AnyAsync(x => x.TerritoryCode == normalizedCode);
            if (territoryExists)
            {
                TempData["ErrorMessage"] = "Territory code already exists.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            _dbContext.Territories.Add(new Territory
            {
                TerritoryCode = normalizedCode,
                TerritoryName = territoryName.Trim(),
                IsActive = true
            });
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Territory added.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Create territory failed");
            TempData["ErrorMessage"] = "Unable to add territory.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Create territory failed");
            TempData["ErrorMessage"] = "Unable to add territory.";
        }

        return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDistributor(string distributorCode, string distributorName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(distributorCode) || string.IsNullOrWhiteSpace(distributorName))
            {
                TempData["ErrorMessage"] = "Distributor code and name are required.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var normalizedCode = distributorCode.Trim();
            var distributorExists = await _dbContext.Distributors.AnyAsync(x => x.DistributorCode == normalizedCode);
            if (distributorExists)
            {
                TempData["ErrorMessage"] = "Distributor code already exists.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            _dbContext.Distributors.Add(new Distributor
            {
                DistributorCode = normalizedCode,
                DistributorName = distributorName.Trim(),
                IsActive = true
            });
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Distributor added.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Create distributor failed");
            TempData["ErrorMessage"] = "Unable to add distributor.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Create distributor failed");
            TempData["ErrorMessage"] = "Unable to add distributor.";
        }

        return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoute(string routeCode, string routeName, int territoryId, int distributorId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(routeCode) || string.IsNullOrWhiteSpace(routeName))
            {
                TempData["ErrorMessage"] = "Route code and name are required.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var territoryExists = await _dbContext.Territories.AnyAsync(x => x.Id == territoryId && x.IsActive);
            var distributorExists = await _dbContext.Distributors.AnyAsync(x => x.Id == distributorId && x.IsActive);
            if (!territoryExists || !distributorExists)
            {
                TempData["ErrorMessage"] = "Route must be mapped to an active territory and distributor.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var normalizedRouteCode = routeCode.Trim();
            var routeExists = await _dbContext.SalesRoutes.AnyAsync(x => x.RouteCode == normalizedRouteCode);
            if (routeExists)
            {
                TempData["ErrorMessage"] = "Route code already exists.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            _dbContext.SalesRoutes.Add(new SalesRoute
            {
                RouteCode = normalizedRouteCode,
                RouteName = routeName.Trim(),
                TerritoryId = territoryId,
                DistributorId = distributorId,
                IsActive = true
            });

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Route added.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Create route failed");
            TempData["ErrorMessage"] = "Unable to add route.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Create route failed");
            TempData["ErrorMessage"] = "Unable to add route.";
        }

        return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
    }
}