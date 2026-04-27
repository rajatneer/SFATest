using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SfaApp.Web.Data;
using SfaApp.Web.Models;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Models.ViewModels;

namespace SfaApp.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is not null)
        {
            if (await _userManager.IsInRoleAsync(user, "SalesRep"))
            {
                return RedirectToAction("Dashboard", "Agent", new { area = "Mobile" });
            }

            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "TSI"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (await _userManager.IsInRoleAsync(user, "DistributorUser"))
            {
                return RedirectToAction("Index", "Orders");
            }
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var model = new DashboardViewModel
        {
            CustomerCount = await _dbContext.Customers.CountAsync(x => x.IsActive),
            ProductCount = await _dbContext.Products.CountAsync(x => x.IsActive),
            PendingApprovalCount = await _dbContext.ApprovalRequests.CountAsync(x => x.Status == ApprovalStatus.Pending),
            TodaysVisitCount = await _dbContext.Visits.CountAsync(x => x.VisitDate == today),
            TodaysOrderCount = await _dbContext.SalesOrders.CountAsync(x => x.OrderDateUtc.Date == DateTime.UtcNow.Date)
        };

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
