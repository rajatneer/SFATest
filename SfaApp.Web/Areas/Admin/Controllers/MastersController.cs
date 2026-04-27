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
        var salesRepRoleId = await _dbContext.Roles
            .Where(x => x.Name == "SalesRep")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var model = new MasterSetupViewModel
        {
            Territories = await _dbContext.Territories.OrderBy(x => x.TerritoryName).ToListAsync(),
            Distributors = await _dbContext.Distributors.OrderBy(x => x.DistributorName).ToListAsync(),
            Routes = await _dbContext.SalesRoutes
                .Include(x => x.Territory)
                .Include(x => x.Distributor)
                .OrderBy(x => x.RouteName)
                .ToListAsync(),
            SalesReps = string.IsNullOrWhiteSpace(salesRepRoleId)
                ? []
                : await (from user in _dbContext.Users
                         join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                         where userRole.RoleId == salesRepRoleId && user.IsActive
                         orderby user.FullName, user.Email
                         select new LookupItemViewModel
                         {
                             Value = user.Id,
                             Text = $"{(user.FullName ?? user.Email ?? user.UserName ?? user.Id)} ({(user.Email ?? user.UserName ?? "-")})"
                         })
                    .Distinct()
                    .ToListAsync(),
            ActiveRouteAssignments = await (from assignment in _dbContext.RouteAssignments
                                            join route in _dbContext.SalesRoutes on assignment.RouteId equals route.Id
                                            join rep in _dbContext.Users on assignment.RepUserId equals rep.Id
                                            where assignment.IsActive
                                            orderby route.RouteName, rep.FullName, rep.Email
                                            select new RouteAssignmentSummaryViewModel
                                            {
                                                RouteId = route.Id,
                                                RouteName = route.RouteName,
                                                RepName = rep.FullName ?? rep.Email ?? rep.UserName ?? rep.Id,
                                                StartDate = assignment.StartDate
                                            })
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRouteAssignment(int routeId, string repUserId, DateOnly? startDate)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            if (routeId <= 0 || string.IsNullOrWhiteSpace(repUserId))
            {
                TempData["ErrorMessage"] = "Route and sales rep are required for assignment.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var route = await _dbContext.SalesRoutes
                .FirstOrDefaultAsync(x => x.Id == routeId && x.IsActive);
            if (route is null)
            {
                TempData["ErrorMessage"] = "Select a valid active route.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var normalizedRepUserId = repUserId.Trim();
            var salesRepRoleId = await _dbContext.Roles
                .Where(x => x.Name == "SalesRep")
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(salesRepRoleId))
            {
                throw new InvalidOperationException("Sales rep role is not configured.");
            }

            var repExists = await (from user in _dbContext.Users
                                   join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                                   where user.Id == normalizedRepUserId && user.IsActive && userRole.RoleId == salesRepRoleId
                                   select user.Id)
                .AnyAsync();

            if (!repExists)
            {
                TempData["ErrorMessage"] = "Select a valid active sales rep.";
                return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
            }

            var effectiveStartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var activeAssignments = await _dbContext.RouteAssignments
                .Where(x => x.RouteId == routeId && x.IsActive && x.RepUserId != normalizedRepUserId)
                .ToListAsync();

            foreach (var activeAssignment in activeAssignments)
            {
                activeAssignment.IsActive = false;
                if (!activeAssignment.EndDate.HasValue || activeAssignment.EndDate.Value >= effectiveStartDate)
                {
                    activeAssignment.EndDate = effectiveStartDate.AddDays(-1);
                }
            }

            var assignment = await _dbContext.RouteAssignments
                .FirstOrDefaultAsync(x => x.RouteId == routeId && x.RepUserId == normalizedRepUserId);

            if (assignment is null)
            {
                assignment = new RouteAssignment
                {
                    RouteId = routeId,
                    RepUserId = normalizedRepUserId
                };
                _dbContext.RouteAssignments.Add(assignment);
            }

            assignment.StartDate = effectiveStartDate;
            assignment.EndDate = null;
            assignment.IsActive = true;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Route assignment updated.";
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Create route assignment failed");
            TempData["ErrorMessage"] = "Unable to assign route.";
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Create route assignment failed");
            TempData["ErrorMessage"] = "Unable to assign route.";
        }
        finally
        {
            await transaction.DisposeAsync();
        }

        return RedirectToAction(nameof(Index), "Masters", new { area = "Admin" });
    }
}