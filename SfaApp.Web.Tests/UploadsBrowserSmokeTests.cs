using System.Globalization;
using Microsoft.Playwright;

namespace SfaApp.Web.Tests;

[Trait("Category", "BrowserSmoke")]
public class UploadsBrowserSmokeTests
{
    [Fact]
    public async Task AdminUploads_DownloadErrorCsv_Smoke()
    {
        var shouldRun = string.Equals(
            Environment.GetEnvironmentVariable("RUN_BROWSER_SMOKE"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!shouldRun)
        {
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable("SMOKE_BASE_URL") ?? "http://127.0.0.1:5076";
        var adminEmail = Environment.GetEnvironmentVariable("SMOKE_ADMIN_EMAIL") ?? "admin@sfa.local";
        var adminPassword = Environment.GetEnvironmentVariable("SMOKE_ADMIN_PASSWORD") ?? "Admin@12345";
        var uniqueCode = $"SMK-{DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}";

        var csvPath = Path.Combine(Path.GetTempPath(), $"upload_smoke_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(
            csvPath,
            $"ProductCode,Sku,Name,UnitPrice,Mrp,IsActive{Environment.NewLine}{uniqueCode},SKU-{uniqueCode},Smoke Product,bad-decimal,100.00,true{Environment.NewLine}");

        var downloadPath = Path.Combine(Path.GetTempPath(), $"upload_error_{Guid.NewGuid():N}.csv");

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{baseUrl}/Identity/Account/Login");
            await page.FillAsync("input[name='Input.Email']", adminEmail);
            await page.FillAsync("input[name='Input.Password']", adminPassword);
            await page.ClickAsync("button[type='submit']");

            await page.GotoAsync($"{baseUrl}/admin/uploads");
            await page.SelectOptionAsync("select[name='UploadType']", "Products");
            await page.SetInputFilesAsync("input[name='UploadFile']", csvPath);
            await page.ClickAsync("button:has-text('Queue')");

            await page.WaitForSelectorAsync("a:has-text('Download Error Report')", new PageWaitForSelectorOptions
            {
                Timeout = 45000
            });

            var downloadTask = page.WaitForDownloadAsync();
            await page.ClickAsync("a:has-text('Download Error Report')");
            var download = await downloadTask;
            await download.SaveAsAsync(downloadPath);

            var downloadedText = await File.ReadAllTextAsync(downloadPath);
            Assert.Contains("RowNumber,Message,RawValues", downloadedText);
            Assert.Contains(uniqueCode, downloadedText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
        }
    }
}
