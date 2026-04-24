namespace SfaApp.Web.Models.Domain;

public class DaySession
{
    public int Id { get; set; }

    public string RepUserId { get; set; } = string.Empty;

    public DateOnly BusinessDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTime StartDayTimestampUtc { get; set; }
    public decimal StartDayLat { get; set; }
    public decimal StartDayLong { get; set; }

    public DateTime? EndDayTimestampUtc { get; set; }
    public decimal? EndDayLat { get; set; }
    public decimal? EndDayLong { get; set; }

    public int? SelectedRouteId { get; set; }
    public SalesRoute? SelectedRoute { get; set; }

    public DaySessionStatus Status { get; set; } = DaySessionStatus.Started;

    public string ClientGeneratedUuid { get; set; } = Guid.NewGuid().ToString();

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Synced;
}