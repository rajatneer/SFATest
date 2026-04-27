using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.ViewModels.Mobile;

namespace SfaApp.Web.Services;

public interface IMobileWorkflowService
{
    Task<DaySession?> GetActiveSessionAsync(string repUserId);
    Task<List<SalesRoute>> GetAssignedRoutesAsync(string repUserId);
    Task<List<Customer>> GetCustomersForSelectedRouteAsync(string repUserId);
    Task<DaySession> StartDayAsync(string repUserId, decimal startLat, decimal startLong, string? startTimeZoneId, int? startUtcOffsetMinutes);
    Task SelectRouteAsync(string repUserId, int routeId);
    Task<Visit> CheckInAsync(string repUserId, CheckInViewModel model);
    Task CheckoutVisitAsync(string repUserId, CheckoutViewModel model);
    Task<SalesOrder> CreateOrderAsync(string repUserId, CreateMobileOrderViewModel model);
    Task EndDayAsync(string repUserId, decimal endLat, decimal endLong, string? endTimeZoneId, int? endUtcOffsetMinutes);
    Task<List<SyncQueueItem>> GetPendingQueueAsync();
}