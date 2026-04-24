using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class ApprovalRequest
{
    public int Id { get; set; }

    [Required]
    public int SalesOrderId { get; set; }

    public SalesOrder? SalesOrder { get; set; }

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public string? RequestedByUserId { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTime? DecidedAtUtc { get; set; }

    public string? DecidedByUserId { get; set; }

    [StringLength(500)]
    public string? DecisionRemarks { get; set; }
}