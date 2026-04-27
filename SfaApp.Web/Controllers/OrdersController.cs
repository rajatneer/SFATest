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
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<OrdersController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var isAdmin = currentUser is not null && await _userManager.IsInRoleAsync(currentUser, "Admin");
        var isDistributorUser = currentUser is not null && await _userManager.IsInRoleAsync(currentUser, "DistributorUser");

        var ordersQuery = _dbContext.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Route)
            .Include(x => x.Distributor)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .AsQueryable();

        if (isDistributorUser && !isAdmin)
        {
            if (!currentUser!.DistributorId.HasValue)
            {
                TempData["ErrorMessage"] = "Distributor mapping is missing for this user.";
                ViewBag.CanManageFulfillment = false;
                return View(new List<SalesOrder>());
            }

            ordersQuery = ordersQuery.Where(x => x.DistributorId == currentUser.DistributorId.Value);
        }

        var orders = await ordersQuery
            .OrderByDescending(x => x.OrderDateUtc)
            .ToListAsync();

        ViewBag.CanManageFulfillment = isAdmin || isDistributorUser;

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
            .Include(x => x.Route)
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

        var mappedDistributorId = customer?.DistributorId ?? customer?.Route?.DistributorId;

        if (customer is not null && customer.RouteId <= 0)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Selected customer is not mapped to a route.");
        }

        if (customer is not null && mappedDistributorId is null)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Selected customer route is not mapped to a distributor.");
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
            RouteId = customer.RouteId,
            DistributorId = mappedDistributorId,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,DistributorUser")]
    public async Task<IActionResult> UpdateFulfillmentStatus(int id, OrderStatus status)
    {
        try
        {
            if (status is not OrderStatus.Accepted and not OrderStatus.Dispatched and not OrderStatus.Delivered)
            {
                TempData["ErrorMessage"] = "Invalid fulfillment status transition.";
                return RedirectToAction(nameof(Index));
            }

            var order = await _dbContext.SalesOrders
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return Forbid();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (!isAdmin)
            {
                var isDistributorUser = await _userManager.IsInRoleAsync(currentUser, "DistributorUser");
                if (!isDistributorUser)
                {
                    return Forbid();
                }

                if (!currentUser.DistributorId.HasValue || order.DistributorId != currentUser.DistributorId.Value)
                {
                    return Forbid();
                }
            }

            if (!CanTransition(order.Status, status))
            {
                TempData["ErrorMessage"] = $"Order status cannot move from {order.Status} to {status}.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = status;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order status updated.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to update fulfillment status for order {OrderId}", id);
            TempData["ErrorMessage"] = "Unable to update order status.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update fulfillment status for order {OrderId}", id);
            TempData["ErrorMessage"] = "Unable to update order status.";
        }

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

    private static bool CanTransition(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        return (currentStatus, nextStatus) switch
        {
            (OrderStatus.Approved, OrderStatus.Accepted) => true,
            (OrderStatus.Created, OrderStatus.Accepted) => true,
            (OrderStatus.Accepted, OrderStatus.Dispatched) => true,
            (OrderStatus.Dispatched, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}