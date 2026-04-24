using Microsoft.AspNetCore.Http;

namespace SfaApp.Web.Services;

public interface IUploadJobProcessingService
{
    Task QueueUploadAsync(string uploadType, IFormFile uploadFile, string uploadedByUserId, CancellationToken cancellationToken = default);
    Task<bool> ProcessNextPendingJobAsync(CancellationToken cancellationToken = default);
}
