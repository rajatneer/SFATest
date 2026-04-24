using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;

namespace SfaApp.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ApprovalsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApprovalsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _dbContext.ApprovalRequests
            .Include(x => x.SalesOrder)
                .ThenInclude(x => x!.Customer)
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderBy(x => x.RequestedAtUtc)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await _dbContext.ApprovalRequests
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request is null || request.SalesOrder is null)
        {
            return NotFound();
        }

        if (request.Status != ApprovalStatus.Pending)
        {
            TempData["ErrorMessage"] = "Request is already processed.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = ApprovalStatus.Approved;
        request.DecidedAtUtc = DateTime.UtcNow;
        request.DecidedByUserId = _userManager.GetUserId(User);
        request.DecisionRemarks = "Approved";

        request.SalesOrder.Status = OrderStatus.Approved;

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Approval request approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? remarks)
    {
        var request = await _dbContext.ApprovalRequests
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request is null || request.SalesOrder is null)
        {
            return NotFound();
        }

        if (request.Status != ApprovalStatus.Pending)
        {
            TempData["ErrorMessage"] = "Request is already processed.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = ApprovalStatus.Rejected;
        request.DecidedAtUtc = DateTime.UtcNow;
        request.DecidedByUserId = _userManager.GetUserId(User);
        request.DecisionRemarks = string.IsNullOrWhiteSpace(remarks) ? "Rejected" : remarks.Trim();

        request.SalesOrder.Status = OrderStatus.Rejected;

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Approval request rejected.";
        return RedirectToAction(nameof(Index));
    }
}