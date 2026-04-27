using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ApplicationDbContext dbContext, ILogger<CustomersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _dbContext.Customers
            .Include(x => x.Route)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(customers);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateRouteOptionsAsync();
        return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await PopulateRouteOptionsAsync();
                return View(customer);
            }

            var route = await _dbContext.SalesRoutes
                .FirstOrDefaultAsync(x => x.Id == customer.RouteId && x.IsActive);

            if (route is null)
            {
                ModelState.AddModelError(nameof(customer.RouteId), "Select a valid active route.");
                await PopulateRouteOptionsAsync();
                return View(customer);
            }

            var outletCodeExists = await _dbContext.Customers
                .AnyAsync(x => x.OutletCode == customer.OutletCode);

            if (outletCodeExists)
            {
                ModelState.AddModelError(nameof(customer.OutletCode), "Outlet code already exists.");
                await PopulateRouteOptionsAsync();
                return View(customer);
            }

            var customerCodeExists = await _dbContext.Customers
                .AnyAsync(x => x.CustomerCode == customer.CustomerCode);

            if (customerCodeExists)
            {
                ModelState.AddModelError(nameof(customer.CustomerCode), "Customer code already exists.");
                await PopulateRouteOptionsAsync();
                return View(customer);
            }

            customer.TerritoryId = route.TerritoryId;
            customer.DistributorId = route.DistributorId;
            customer.CreatedAtUtc = DateTime.UtcNow;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create customer");
            TempData["ErrorMessage"] = "Unable to create customer.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create customer");
            TempData["ErrorMessage"] = "Unable to create customer.";
        }

        await PopulateRouteOptionsAsync();
        return View(customer);
    }

    private async Task PopulateRouteOptionsAsync()
    {
        var routeOptions = await _dbContext.SalesRoutes
            .Where(x => x.IsActive)
            .OrderBy(x => x.RouteName)
            .Select(x => new
            {
                x.Id,
                Label = $"{x.RouteCode} - {x.RouteName}"
            })
            .ToListAsync();

        ViewBag.RouteOptions = new SelectList(routeOptions, "Id", "Label");
    }
}