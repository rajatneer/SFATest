using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Services;

public class HttpSyncTransportClient : ISyncTransportClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SyncTransportOptions> _syncTransportOptions;
    private readonly ILogger<HttpSyncTransportClient> _logger;

    public HttpSyncTransportClient(
        HttpClient httpClient,
        IOptions<SyncTransportOptions> syncTransportOptions,
        ILogger<HttpSyncTransportClient> logger)
    {
        _httpClient = httpClient;
        _syncTransportOptions = syncTransportOptions;
        _logger = logger;
    }

    public async Task<SyncTransportResult> PushAsync(SyncQueueItem item, JsonDocument payload, CancellationToken cancellationToken = default)
    {
        var options = _syncTransportOptions.Value;
        if (!options.Enabled)
        {
            return SyncTransportResult.Success("Sync transport is disabled.");
        }

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return SyncTransportResult.Failure("Sync transport base URL is missing.");
        }

        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));

            var endpointPath = options.EndpointPath;
            if (string.IsNullOrWhiteSpace(endpointPath))
            {
                endpointPath = "/api/sync/push";
            }

            var baseUri = new Uri(options.BaseUrl, UriKind.Absolute);
            var requestUri = new Uri(baseUri, endpointPath);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                request.Headers.TryAddWithoutValidation("X-Api-Key", options.ApiKey);
            }

            request.Content = JsonContent.Create(new SyncPushRequest
            {
                EntityType = item.EntityType,
                EntityClientUuid = item.EntityClientUuid,
                CreatedAtUtc = item.CreatedAtUtc,
                Payload = payload.RootElement.Clone()
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return SyncTransportResult.Success();
            }

            if ((int)response.StatusCode == 409)
            {
                return SyncTransportResult.Conflict($"Transport returned conflict (409). {responseContent}");
            }

            return SyncTransportResult.Failure($"Transport returned {(int)response.StatusCode}. {responseContent}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while pushing sync item {SyncQueueItemId}", item.Id);
            return SyncTransportResult.Failure(ex.ToString());
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "HTTP timeout while pushing sync item {SyncQueueItemId}", item.Id);
            return SyncTransportResult.Failure(ex.ToString());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while pushing sync item {SyncQueueItemId}", item.Id);
            return SyncTransportResult.Failure(ex.ToString());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error while pushing sync item {SyncQueueItemId}", item.Id);
            return SyncTransportResult.Failure(ex.ToString());
        }
    }

    private sealed class SyncPushRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityClientUuid { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public JsonElement Payload { get; set; }
    }
}
