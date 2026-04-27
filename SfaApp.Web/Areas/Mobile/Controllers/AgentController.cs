using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;
using SfaApp.Web.Models.ViewModels.Mobile;
using SfaApp.Web.Services;

namespace SfaApp.Web.Areas.Mobile.Controllers;

[Area("Mobile")]
public class AgentController : Controller
{
    private readonly IMobileWorkflowService _mobileWorkflowService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IMobileWorkflowService mobileWorkflowService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ILogger<AgentController> logger)
    {
        _mobileWorkflowService = mobileWorkflowService;
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var signedInUser = await _userManager.GetUserAsync(User);
            if (signedInUser is not null && await _userManager.IsInRoleAsync(signedInUser, "SalesRep"))
            {
                return RedirectToAction(nameof(Dashboard));
            }

            await _signInManager.SignOutAsync();
            // Force a new anonymous request so antiforgery tokens are issued for the current auth state.
            return RedirectToAction(nameof(Login));
        }

        return View(new MobileLoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(MobileLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Username);
            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(model);
            }

            if (!await _userManager.IsInRoleAsync(user, "SalesRep"))
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "This account is not allowed for mobile agent access.");
                return View(model);
            }

            return RedirectToAction(nameof(Dashboard));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Mobile login failed for {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Login failed.");
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Login));
        }

        var session = await _mobileWorkflowService.GetActiveSessionAsync(userId);
        var todaySession = await _dbContext.DaySessions
            .Where(x => x.RepUserId == userId && x.BusinessDate == DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(x => x.StartDayTimestampUtc)
            .FirstOrDefaultAsync();
        var pendingQueue = await _mobileWorkflowService.GetPendingQueueAsync();

        var model = new MobileDashboardViewModel
        {
            ActiveSession = session,
            ActiveRouteName = session?.SelectedRoute?.RouteName,
            StartDayCheckInDisplay = FormatClientTime(
                todaySession?.StartDayTimestampUtc,
                todaySession?.StartDayTimeZoneId,
                todaySession?.StartDayUtcOffsetMinutes),
            EndDayCheckOutDisplay = FormatClientTime(
                todaySession?.EndDayTimestampUtc,
                todaySession?.EndDayTimeZoneId,
                todaySession?.EndDayUtcOffsetMinutes),
            PendingSyncCount = pendingQueue.Count,
            VisitedCustomers = await _dbContext.Visits.CountAsync(x => x.RepUserId == userId && x.VisitDate == DateOnly.FromDateTime(DateTime.UtcNow)),
            OrdersCreated = await _dbContext.SalesOrders.CountAsync(x => x.RepUserId == userId && x.OrderDateUtc.Date == DateTime.UtcNow.Date)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartDay(
        decimal startLat = 0,
        decimal startLong = 0,
        string? startTimeZoneId = null,
        int? startUtcOffsetMinutes = null)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Login));
        }

        try
        {
            await _mobileWorkflowService.StartDayAsync(userId, startLat, startLong, startTimeZoneId, startUtcOffsetMinutes);
            TempData["SuccessMessage"] = "Day started.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Start day failed for user {UserId}", userId);
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while starting day for user {UserId}", userId);
            TempData["ErrorMessage"] = "Unable to start day.";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndDay(
        decimal endLat = 0,
        decimal endLong = 0,
        string? endTimeZoneId = null,
        int? endUtcOffsetMinutes = null)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Login));
        }

        try
        {
            await _mobileWorkflowService.EndDayAsync(userId, endLat, endLong, endTimeZoneId, endUtcOffsetMinutes);
            TempData["SuccessMessage"] = "Day ended.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "End day failed for user {UserId}", userId);
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while ending day for user {UserId}", userId);
            TempData["ErrorMessage"] = "Unable to end day.";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> RouteSelection()
    {
        var userId = _userManager.GetUserId(User)!;
        var routes = await _mobileWorkflowService.GetAssignedRoutesAsync(userId);
        return View(routes);
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectRoute(int routeId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Login));
        }

        try
        {
            await _mobileWorkflowService.SelectRouteAsync(userId, routeId);
            TempData["SuccessMessage"] = "Route selected.";
            return RedirectToAction(nameof(CustomerList));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Route selection failed for user {UserId}, route {RouteId}", userId, routeId);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(RouteSelection));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while selecting route for user {UserId}, route {RouteId}", userId, routeId);
            TempData["ErrorMessage"] = "Unable to select route.";
            return RedirectToAction(nameof(RouteSelection));
        }
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> CustomerList()
    {
        var userId = _userManager.GetUserId(User)!;
        var customers = await _mobileWorkflowService.GetCustomersForSelectedRouteAsync(userId);
        return View(customers);
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> CustomerDetail(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var session = await _mobileWorkflowService.GetActiveSessionAsync(userId);
        if (session?.SelectedRouteId is null)
        {
            TempData["ErrorMessage"] = "Select a route to continue.";
            return RedirectToAction(nameof(RouteSelection));
        }

        var customer = await _dbContext.Customers
            .Include(x => x.Route)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.RouteId == session.SelectedRouteId);

        if (customer is null)
        {
            TempData["ErrorMessage"] = "Customer is outside your active route.";
            return RedirectToAction(nameof(CustomerList));
        }

        return View(customer);
    }

    [Authorize(Roles = "SalesRep")]
    public IActionResult CheckIn(int customerId)
    {
        return View(new CheckInViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;

        try
        {
            var visit = await _mobileWorkflowService.CheckInAsync(userId, model);
            TempData["SuccessMessage"] = "Check-in successful.";
            return RedirectToAction(nameof(VisitCheckout), new { visitId = visit.Id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Check-in failed");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(CustomerDetail), new { id = model.CustomerId });
        }
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> VisitCheckout(int visitId)
    {
        var userId = _userManager.GetUserId(User)!;
        var visit = await _dbContext.Visits
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == visitId && x.RepUserId == userId);

        if (visit is null)
        {
            return NotFound();
        }

        ViewBag.Visit = visit;
        return View(new CheckoutViewModel { VisitId = visitId });
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VisitCheckout(CheckoutViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;

        if (!ModelState.IsValid)
        {
            var existingVisit = await _dbContext.Visits
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == model.VisitId && x.RepUserId == userId);
            ViewBag.Visit = existingVisit;
            return View(model);
        }

        try
        {
            await _mobileWorkflowService.CheckoutVisitAsync(userId, model);
            TempData["SuccessMessage"] = "Visit checkout saved.";
            return RedirectToAction(nameof(CustomerList));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Visit checkout failed for user {UserId}, visit {VisitId}", userId, model.VisitId);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(CustomerList));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while checkout for user {UserId}, visit {VisitId}", userId, model.VisitId);
            TempData["ErrorMessage"] = "Unable to save checkout.";
            return RedirectToAction(nameof(CustomerList));
        }
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> OrderCreation(int customerId, int? visitId)
    {
        var userId = _userManager.GetUserId(User)!;
        var session = await _mobileWorkflowService.GetActiveSessionAsync(userId);
        if (session?.SelectedRouteId is null)
        {
            TempData["ErrorMessage"] = "Select a route before creating an order.";
            return RedirectToAction(nameof(RouteSelection));
        }

        var customerExistsOnRoute = await _dbContext.Customers.AnyAsync(x =>
            x.Id == customerId &&
            x.IsActive &&
            x.RouteId == session.SelectedRouteId);

        if (!customerExistsOnRoute)
        {
            TempData["ErrorMessage"] = "Customer is outside your active route.";
            return RedirectToAction(nameof(CustomerList));
        }

        if (visitId.HasValue)
        {
            var visitExists = await _dbContext.Visits.AnyAsync(x => x.Id == visitId.Value && x.RepUserId == userId && x.CustomerId == customerId);
            if (!visitExists)
            {
                TempData["ErrorMessage"] = "Selected visit is invalid.";
                return RedirectToAction(nameof(CustomerDetail), new { id = customerId });
            }
        }

        await PopulateProductOptionsAsync();

        return View(new CreateMobileOrderViewModel
        {
            CustomerId = customerId,
            VisitId = visitId
        });
    }

    [HttpPost]
    [Authorize(Roles = "SalesRep")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderCreation(CreateMobileOrderViewModel model)
    {
        EnsureAtLeastOneOrderLine(model);

        if (!ModelState.IsValid)
        {
            await PopulateProductOptionsAsync();
            return View(model);
        }

        var userId = _userManager.GetUserId(User)!;
        try
        {
            var order = await _mobileWorkflowService.CreateOrderAsync(userId, model);
            return RedirectToAction(nameof(OrderSummary), new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Order creation failed for user {UserId}, customer {CustomerId}", userId, model.CustomerId);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(CustomerDetail), new { id = model.CustomerId });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating order for user {UserId}, customer {CustomerId}", userId, model.CustomerId);
            TempData["ErrorMessage"] = "Unable to create order.";
            return RedirectToAction(nameof(CustomerDetail), new { id = model.CustomerId });
        }
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> OrderSummary(int orderId)
    {
        var order = await _dbContext.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Distributor)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [Authorize(Roles = "SalesRep")]
    public async Task<IActionResult> PendingSyncQueue()
    {
        var pendingItems = await _mobileWorkflowService.GetPendingQueueAsync();
        return View(pendingItems);
    }

    private async Task PopulateProductOptionsAsync()
    {
        var products = await _dbContext.Products.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
        ViewBag.ProductOptions = new SelectList(products, nameof(Product.Id), nameof(Product.Name));
    }

    private static void EnsureAtLeastOneOrderLine(CreateMobileOrderViewModel model)
    {
        model.OrderLines ??= [];
        if (model.OrderLines.Count == 0)
        {
            model.OrderLines.Add(new CreateMobileOrderLineViewModel());
        }
    }

    private string? FormatClientTime(DateTime? utcTimestamp, string? timeZoneId, int? utcOffsetMinutes)
    {
        if (!utcTimestamp.HasValue)
        {
            return null;
        }

        var utcValue = DateTime.SpecifyKind(utcTimestamp.Value, DateTimeKind.Utc);
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var localValue = TimeZoneInfo.ConvertTimeFromUtc(utcValue, timezone);
                return $"{localValue:dd-MMM HH:mm} ({timezone.Id})";
            }
            catch (TimeZoneNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unknown timezone id {TimeZoneId} while formatting dashboard time", timeZoneId);
            }
            catch (InvalidTimeZoneException ex)
            {
                _logger.LogWarning(ex, "Invalid timezone id {TimeZoneId} while formatting dashboard time", timeZoneId);
            }
        }

        if (utcOffsetMinutes.HasValue)
        {
            var localByOffset = utcValue.AddMinutes(-utcOffsetMinutes.Value);
            return $"{localByOffset:dd-MMM HH:mm} (UTC{FormatOffset(utcOffsetMinutes.Value)})";
        }

        return $"{utcValue:dd-MMM HH:mm} (UTC)";
    }

    private static string FormatOffset(int utcOffsetMinutes)
    {
        var total = -utcOffsetMinutes;
        var sign = total >= 0 ? "+" : "-";
        var absolute = Math.Abs(total);
        var hours = absolute / 60;
        var minutes = absolute % 60;
        return $"{sign}{hours:00}:{minutes:00}";
    }
}
