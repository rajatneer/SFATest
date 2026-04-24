using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Models.ViewModels;

namespace SfaApp.Web.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public VisitsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var visits = await _dbContext.Visits
            .Include(x => x.Customer)
            .OrderByDescending(x => x.VisitDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return View(visits);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCustomersAsync();
        return View(new CreateVisitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVisitViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCustomersAsync();
            return View(model);
        }

        var customerExists = await _dbContext.Customers
            .AnyAsync(x => x.Id == model.CustomerId && x.IsActive);

        if (!customerExists)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Select a valid active customer.");
            await PopulateCustomersAsync();
            return View(model);
        }

        var visit = new Visit
        {
            CustomerId = model.CustomerId,
            VisitDate = model.VisitDate,
            Outcome = model.Outcome,
            Notes = model.Notes,
            CreatedByUserId = _userManager.GetUserId(User),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Visits.Add(visit);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Visit logged successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCustomersAsync()
    {
        var customers = await _dbContext.Customers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Customers = new SelectList(customers, nameof(Customer.Id), nameof(Customer.Name));
    }
}