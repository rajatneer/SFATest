using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CustomersController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _dbContext.Customers
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(customers);
    }

    public IActionResult Create()
    {
        return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        var outletCodeExists = await _dbContext.Customers
            .AnyAsync(x => x.OutletCode == customer.OutletCode);

        if (outletCodeExists)
        {
            ModelState.AddModelError(nameof(customer.OutletCode), "Outlet code already exists.");
            return View(customer);
        }

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Customer created successfully.";
        return RedirectToAction(nameof(Index));
    }
}