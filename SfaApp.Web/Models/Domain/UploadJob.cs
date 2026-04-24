using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class UploadJob
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string UploadType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FileName { get; set; } = string.Empty;

    public string UploadedByUserId { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public UploadJobStatus Status { get; set; } = UploadJobStatus.Pending;

    [StringLength(250)]
    public string? ErrorFilePath { get; set; }
}