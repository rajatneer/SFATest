using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.ViewModels.Admin;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,TSI")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext dbContext, ILogger<HomeController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var model = new AdminDashboardViewModel
            {
                UserCount = await _dbContext.Users.CountAsync(),
                TerritoryCount = await _dbContext.Territories.CountAsync(x => x.IsActive),
                RouteCount = await _dbContext.SalesRoutes.CountAsync(x => x.IsActive),
                CustomerCount = await _dbContext.Customers.CountAsync(x => x.IsActive),
                UploadJobsPending = await _dbContext.UploadJobs.CountAsync(x => x.Status == UploadJobStatus.Pending)
            };

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to load admin dashboard");
            TempData["ErrorMessage"] = "Unable to load dashboard.";
            return View(new AdminDashboardViewModel());
        }
    }
}