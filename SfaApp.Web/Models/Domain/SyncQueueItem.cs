using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class SyncQueueItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string EntityClientUuid { get; set; } = string.Empty;

    [Required]
    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int RetryCount { get; set; }
    public DateTime? LastRetryAtUtc { get; set; }

    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    [StringLength(500)]
    public string? LastErrorMessage { get; set; }
}