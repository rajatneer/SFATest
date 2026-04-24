using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class Visit
{
    public int Id { get; set; }

    public string? RepUserId { get; set; }

    public int? DaySessionId { get; set; }
    public DaySession? DaySession { get; set; }

    public int? RouteId { get; set; }
    public SalesRoute? Route { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Required]
    public DateOnly VisitDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [StringLength(200)]
    public string? Outcome { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime? CheckinTimestampUtc { get; set; }
    public decimal? CheckinLat { get; set; }
    public decimal? CheckinLong { get; set; }

    public decimal? CustomerRefLat { get; set; }
    public decimal? CustomerRefLong { get; set; }
    public decimal? GeoDistanceMeters { get; set; }
    public bool? WithinToleranceFlag { get; set; }
    public bool CoordinateCapturedDuringVisitFlag { get; set; }

    public DateTime? CheckoutTimestampUtc { get; set; }
    public decimal? CheckoutLat { get; set; }
    public decimal? CheckoutLong { get; set; }

    public VisitStatus VisitStatus { get; set; } = VisitStatus.Completed;

    public string ClientGeneratedUuid { get; set; } = Guid.NewGuid().ToString();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Synced;

    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}