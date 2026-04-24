using System.Text.Json;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Services;

public interface ISyncTransportClient
{
    Task<SyncTransportResult> PushAsync(SyncQueueItem item, JsonDocument payload, CancellationToken cancellationToken = default);
}

public sealed class SyncTransportResult
{
    public bool IsSuccess { get; init; }
    public bool IsConflict { get; init; }
    public string? Message { get; init; }

    public static SyncTransportResult Success(string? message = null)
    {
        return new SyncTransportResult { IsSuccess = true, Message = message };
    }

    public static SyncTransportResult Conflict(string? message = null)
    {
        return new SyncTransportResult { IsConflict = true, Message = message };
    }

    public static SyncTransportResult Failure(string? message = null)
    {
        return new SyncTransportResult { Message = message };
    }
}

public sealed class SyncTransportOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointPath { get; set; } = "/api/sync/push";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
