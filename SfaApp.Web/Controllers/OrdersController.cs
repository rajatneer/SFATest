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
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _dbContext.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.OrderDateUtc)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new CreateSalesOrderViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync();
            return View(model);
        }

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(x => x.Id == model.CustomerId && x.IsActive);

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == model.ProductId && x.IsActive);

        if (customer is null)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Select a valid active customer.");
        }

        if (product is null)
        {
            ModelState.AddModelError(nameof(model.ProductId), "Select a valid active product.");
        }

        if (!ModelState.IsValid || customer is null || product is null)
        {
            await PopulateLookupsAsync();
            return View(model);
        }

        var lineTotal = product.UnitPrice * model.Quantity;

        var order = new SalesOrder
        {
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            CustomerId = customer.Id,
            OrderDateUtc = DateTime.UtcNow,
            Status = OrderStatus.PendingApproval,
            Notes = model.Notes,
            TotalAmount = lineTotal,
            Lines =
            [
                new SalesOrderLine
                {
                    ProductId = product.Id,
                    Quantity = model.Quantity,
                    UnitPrice = product.UnitPrice,
                    LineTotal = lineTotal
                }
            ],
            ApprovalRequest = new ApprovalRequest
            {
                RequestedAtUtc = DateTime.UtcNow,
                RequestedByUserId = _userManager.GetUserId(User),
                Status = ApprovalStatus.Pending
            }
        };

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Sales order created and submitted for approval.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync()
    {
        var customers = await _dbContext.Customers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var products = await _dbContext.Products
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Customers = new SelectList(customers, nameof(Customer.Id), nameof(Customer.Name));
        ViewBag.Products = new SelectList(products, nameof(Product.Id), nameof(Product.Name));
    }
}