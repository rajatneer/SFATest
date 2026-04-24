namespace SfaApp.Web.Services;

public interface ISyncQueueProcessingService
{
    Task<int> ProcessDueItemsAsync(CancellationToken cancellationToken = default);
}
