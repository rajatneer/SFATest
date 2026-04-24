namespace SfaApp.Web.Models.Domain;

public class RouteAssignment
{
    public int Id { get; set; }

    public int RouteId { get; set; }
    public SalesRoute? Route { get; set; }

    public string RepUserId { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}