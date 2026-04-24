using Microsoft.EntityFrameworkCore;

namespace SfaApp.Web.Services;

public class SyncQueueBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncQueueBackgroundService> _logger;

    public SyncQueueBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncQueueBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncProcessor = scope.ServiceProvider.GetRequiredService<ISyncQueueProcessingService>();
                var processed = await syncProcessor.ProcessDueItemsAsync(stoppingToken);

                var delay = processed > 0 ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(30);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Sync queue background processor encountered a database error");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Sync queue background processor encountered an invalid operation");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
