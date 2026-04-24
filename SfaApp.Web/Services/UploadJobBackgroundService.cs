using Microsoft.EntityFrameworkCore;

namespace SfaApp.Web.Services;

public class UploadJobBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UploadJobBackgroundService> _logger;

    public UploadJobBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<UploadJobBackgroundService> logger)
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
                var processingService = scope.ServiceProvider.GetRequiredService<IUploadJobProcessingService>();
                var processed = await processingService.ProcessNextPendingJobAsync(stoppingToken);

                var delay = processed ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(10);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Upload background processor encountered a database error");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Upload background processor encountered an invalid operation");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Upload background processor encountered an I/O error");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
