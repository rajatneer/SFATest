using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.ViewModels.Admin;
using SfaApp.Web.Services;

namespace SfaApp.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UploadsController : Controller
{
    private static readonly string[] AllowedUploadTypes = ["Customers", "Routes", "Products", "PriceList", "RouteAssignments"];

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IUploadJobProcessingService _uploadJobProcessingService;
    private readonly ILogger<UploadsController> _logger;

    public UploadsController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        IUploadJobProcessingService uploadJobProcessingService,
        ILogger<UploadsController> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _uploadJobProcessingService = uploadJobProcessingService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var jobs = await _dbContext.UploadJobs.OrderByDescending(x => x.UploadedAtUtc).ToListAsync();
        ViewBag.UploadTypes = AllowedUploadTypes;
        return View(jobs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUploadJobViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide valid upload job data.";
            return RedirectToAction(nameof(Index), "Uploads", new { area = "Admin" });
        }

        try
        {
            var normalizedUploadType = AllowedUploadTypes
                .FirstOrDefault(x => string.Equals(x, model.UploadType?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (normalizedUploadType is null)
            {
                TempData["ErrorMessage"] = "Invalid upload type.";
                return RedirectToAction(nameof(Index), "Uploads", new { area = "Admin" });
            }

            if (model.UploadFile is null || model.UploadFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please choose a file to upload.";
                return RedirectToAction(nameof(Index), "Uploads", new { area = "Admin" });
            }

            await _uploadJobProcessingService.QueueUploadAsync(
                normalizedUploadType,
                model.UploadFile,
                User.Identity?.Name ?? "system");

            TempData["SuccessMessage"] = "Upload job recorded.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Upload job create failed");
            TempData["ErrorMessage"] = "Unable to save upload job.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid upload job create operation");
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while queueing upload job");
            TempData["ErrorMessage"] = "Unable to store upload file.";
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while queueing upload job");
            TempData["ErrorMessage"] = "Access denied while storing upload file.";
        }

        return RedirectToAction(nameof(Index), "Uploads", new { area = "Admin" });
    }

    public async Task<IActionResult> DownloadError(int id)
    {
        var job = await _dbContext.UploadJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job is null || string.IsNullOrWhiteSpace(job.ErrorFilePath))
        {
            return NotFound();
        }

        try
        {
            var relativePath = job.ErrorFilePath.Replace('/', Path.DirectorySeparatorChar);
            var contentRoot = Path.GetFullPath(_environment.ContentRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(contentRoot, relativePath));

            if (!fullPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var contentType = Path.GetExtension(fullPath).Equals(".csv", StringComparison.OrdinalIgnoreCase)
                ? "text/csv"
                : "text/plain";

            return PhysicalFile(fullPath, contentType, Path.GetFileName(fullPath));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while downloading upload error file for job {UploadJobId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while downloading upload error file for job {UploadJobId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while downloading upload error file for job {UploadJobId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}