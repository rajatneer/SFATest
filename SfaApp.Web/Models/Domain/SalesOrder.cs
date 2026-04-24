using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class SalesOrder
{
    public int Id { get; set; }

    public int? DaySessionId { get; set; }
    public DaySession? DaySession { get; set; }

    public int? RouteId { get; set; }
    public SalesRoute? Route { get; set; }

    public int? DistributorId { get; set; }
    public Distributor? Distributor { get; set; }

    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }

    public string? RepUserId { get; set; }

    [Required]
    [StringLength(30)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.PendingApproval;

    public decimal TotalAmount { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }

    [StringLength(20)]
    public string Source { get; set; } = "web";

    public string ClientGeneratedUuid { get; set; } = Guid.NewGuid().ToString();
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Synced;

    [StringLength(500)]
    public string? Notes { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();

    public ApprovalRequest? ApprovalRequest { get; set; }
}