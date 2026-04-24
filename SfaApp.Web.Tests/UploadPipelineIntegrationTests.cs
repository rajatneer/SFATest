using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Services;
using SfaApp.Web.Tests.Infrastructure;

namespace SfaApp.Web.Tests;

[Trait("Category", "Integration")]
public class UploadPipelineIntegrationTests
{
    [Fact]
    public async Task ProcessProductsUpload_CompletesAndCommitsRows()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<IUploadJobProcessingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        ResetUploadArtifacts(environment);

        var uploadFile = CreateCsvFile(
            $"products_valid_{Guid.NewGuid():N}.csv",
            "ProductCode,Sku,Name,UnitPrice,Mrp,IsActive\nP-100,SKU-100,Product 100,120.50,150.00,true\n");

        await processor.QueueUploadAsync("Products", uploadFile, "integration-admin");
        var processed = await processor.ProcessNextPendingJobAsync();

        Assert.True(processed);

        var job = await dbContext.UploadJobs.OrderByDescending(x => x.Id).FirstAsync();
        Assert.Equal(UploadJobStatus.Completed, job.Status);
        Assert.Null(job.ErrorFilePath);

        var product = await dbContext.Products.SingleAsync(x => x.ProductCode == "P-100");
        Assert.Equal(120.50m, product.UnitPrice);
        Assert.Equal(150.00m, product.Mrp);
    }

    [Fact]
    public async Task ProcessProductsUpload_WithRowValidationErrors_WritesDownloadableCsv()
    {
        using var factory = new IntegrationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<IUploadJobProcessingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        ResetUploadArtifacts(environment);

        var uploadFile = CreateCsvFile(
            $"products_invalid_{Guid.NewGuid():N}.csv",
            "ProductCode,Sku,Name,UnitPrice,Mrp,IsActive\nP-ERR,SKU-ERR,Product Err,not-a-decimal,150.00,true\n");

        await processor.QueueUploadAsync("Products", uploadFile, "integration-admin");
        var processed = await processor.ProcessNextPendingJobAsync();

        Assert.True(processed);

        var job = await dbContext.UploadJobs.OrderByDescending(x => x.Id).FirstAsync();
        Assert.Equal(UploadJobStatus.Failed, job.Status);
        Assert.False(string.IsNullOrWhiteSpace(job.ErrorFilePath));

        var fullErrorPath = Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            job.ErrorFilePath!.Replace('/', Path.DirectorySeparatorChar)));

        Assert.True(File.Exists(fullErrorPath));

        var errorCsv = await File.ReadAllTextAsync(fullErrorPath);
        Assert.Contains("RowNumber,Message,RawValues", errorCsv);
        Assert.Contains("invalid decimal value in UnitPrice", errorCsv, StringComparison.OrdinalIgnoreCase);

        Assert.False(await dbContext.Products.AnyAsync(x => x.ProductCode == "P-ERR"));
    }

    private static IFormFile CreateCsvFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "uploadFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    private static void ResetUploadArtifacts(IWebHostEnvironment environment)
    {
        var uploadsDir = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
        var uploadErrorsDir = Path.Combine(environment.ContentRootPath, "App_Data", "upload-errors");

        if (Directory.Exists(uploadsDir))
        {
            Directory.Delete(uploadsDir, true);
        }

        if (Directory.Exists(uploadErrorsDir))
        {
            Directory.Delete(uploadErrorsDir, true);
        }
    }
}
