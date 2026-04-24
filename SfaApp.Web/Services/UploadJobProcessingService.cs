using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Services;

public class UploadJobProcessingService : IUploadJobProcessingService
{
    private static readonly string[] AllowedUploadTypes = ["Customers", "Routes", "Products", "PriceList", "RouteAssignments"];
    private static readonly string[] AllowedExtensions = [".csv", ".xlsx"];

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadJobProcessingService> _logger;

    public UploadJobProcessingService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        ILogger<UploadJobProcessingService> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
    }

    public async Task QueueUploadAsync(string uploadType, IFormFile uploadFile, string uploadedByUserId, CancellationToken cancellationToken = default)
    {
        var normalizedUploadType = AllowedUploadTypes
            .FirstOrDefault(x => string.Equals(x, uploadType?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalizedUploadType is null)
        {
            throw new InvalidOperationException("Invalid upload type.");
        }

        var normalizedFileName = Path.GetFileName(uploadFile.FileName.Trim());
        var extension = Path.GetExtension(normalizedFileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only .csv and .xlsx files are supported.");
        }

        var duplicatePendingJobExists = await _dbContext.UploadJobs.AnyAsync(x =>
            x.UploadType == normalizedUploadType &&
            x.FileName == normalizedFileName &&
            (x.Status == UploadJobStatus.Pending || x.Status == UploadJobStatus.Processing), cancellationToken);

        if (duplicatePendingJobExists)
        {
            throw new InvalidOperationException("A pending upload job already exists for this file.");
        }

        UploadJob job;
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                job = new UploadJob
                {
                    UploadType = normalizedUploadType,
                    FileName = normalizedFileName,
                    UploadedByUserId = uploadedByUserId,
                    UploadedAtUtc = DateTime.UtcNow,
                    Status = UploadJobStatus.Pending
                };

                _dbContext.UploadJobs.Add(job);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to queue upload job for type {UploadType} and file {FileName}", normalizedUploadType, normalizedFileName);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Invalid operation while queueing upload job for type {UploadType} and file {FileName}", normalizedUploadType, normalizedFileName);
                throw;
            }
        }

        try
        {
            var targetPath = BuildUploadFilePath(job.Id, normalizedFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            await using var stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await uploadFile.CopyToAsync(stream, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to store upload file for job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to store upload file for job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
            throw;
        }
    }

    public async Task<bool> ProcessNextPendingJobAsync(CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.UploadJobs
            .Where(x => x.Status == UploadJobStatus.Pending)
            .OrderBy(x => x.UploadedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            return false;
        }

        try
        {
            job.Status = UploadJobStatus.Processing;
            job.ErrorFilePath = null;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var filePath = ResolveUploadFilePath(job.Id);
            if (filePath is null)
            {
                throw new FileNotFoundException($"Upload source file was not found for job {job.Id}.");
            }

            var rows = await ParseRowsAsync(filePath, cancellationToken);
            List<UploadValidationError> validationErrors;
            switch (job.UploadType)
            {
                case "Customers":
                    validationErrors = await CommitCustomersAsync(rows, cancellationToken);
                    break;
                case "Routes":
                    validationErrors = await CommitRoutesAsync(rows, cancellationToken);
                    break;
                case "Products":
                    validationErrors = await CommitProductsAsync(rows, cancellationToken);
                    break;
                case "PriceList":
                    validationErrors = await CommitPriceListAsync(rows, cancellationToken);
                    break;
                case "RouteAssignments":
                    validationErrors = await CommitRouteAssignmentsAsync(rows, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported upload type: {job.UploadType}");
            }

            if (validationErrors.Count > 0)
            {
                await MarkJobFailedAsync(job.Id, validationErrors, cancellationToken);
                return true;
            }

            job.Status = UploadJobStatus.Completed;
            job.ErrorFilePath = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while processing upload job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Upload validation failed for job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Upload parsing failed for job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while processing upload job {UploadJobId}", job.Id);
            await MarkJobFailedAsync(job.Id, ex.ToString(), cancellationToken);
        }

        return true;
    }

    private async Task<List<UploadRow>> ParseRowsAsync(string filePath, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filePath);

        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return await ParseCsvRowsAsync(filePath, cancellationToken);
        }

        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return ParseXlsxRows(filePath);
        }

        throw new InvalidOperationException("Unsupported upload file format.");
    }

    private async Task<List<UploadRow>> ParseCsvRowsAsync(string filePath, CancellationToken cancellationToken)
    {
        var rows = new List<UploadRow>();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException("Upload file has no header row.");
        }

        var headers = SplitCsvLine(headerLine).Select(x => x.Trim()).ToArray();
        var rowNumber = 1;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = SplitCsvLine(line);
            var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                var value = i < values.Count ? values[i]?.Trim() ?? string.Empty : string.Empty;
                rowValues[headers[i]] = value;
            }

            rows.Add(new UploadRow(rowNumber, rowValues));
        }

        return rows;
    }

    private static List<UploadRow> ParseXlsxRows(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            throw new InvalidOperationException("Upload workbook has no worksheet.");
        }

        var firstRow = worksheet.FirstRowUsed();
        var lastRow = worksheet.LastRowUsed();

        if (firstRow is null || lastRow is null)
        {
            throw new InvalidOperationException("Upload workbook has no data.");
        }

        var headerCells = firstRow.CellsUsed().ToList();
        if (headerCells.Count == 0)
        {
            throw new InvalidOperationException("Upload workbook has empty headers.");
        }

        var headers = headerCells.Select(x => x.GetString().Trim()).ToList();
        var rows = new List<UploadRow>();

        for (var rowNumber = firstRow.RowNumber() + 1; rowNumber <= lastRow.RowNumber(); rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (row.CellsUsed().All(x => string.IsNullOrWhiteSpace(x.GetString())))
            {
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                values[headers[i]] = row.Cell(i + 1).GetString().Trim();
            }

            rows.Add(new UploadRow(rowNumber, values));
        }

        return rows;
    }

    private async Task<List<UploadValidationError>> CommitCustomersAsync(List<UploadRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Customers upload has no data rows.");
        }

        var validationErrors = new List<UploadValidationError>();

        var routeCodes = rows
            .Select(x => GetRequiredValue(x, "RouteCode"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var routes = await _dbContext.SalesRoutes
            .Where(x => routeCodes.Contains(x.RouteCode))
            .ToListAsync(cancellationToken);

        var routeMap = routes.ToDictionary(x => x.RouteCode, StringComparer.OrdinalIgnoreCase);

        var customerCodes = rows
            .Select(x => GetRequiredValue(x, "CustomerCode"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingCustomers = await _dbContext.Customers
            .Where(x => customerCodes.Contains(x.CustomerCode))
            .ToListAsync(cancellationToken);

        var customerMap = existingCustomers.ToDictionary(x => x.CustomerCode, StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    var customerCode = GetRequiredValue(row, "CustomerCode");
                    if (!customerMap.TryGetValue(customerCode, out var customer))
                    {
                        customer = new Customer
                        {
                            CustomerCode = customerCode,
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        customerMap[customerCode] = customer;
                        _dbContext.Customers.Add(customer);
                    }

                    var routeCode = GetRequiredValue(row, "RouteCode");
                    if (!routeMap.TryGetValue(routeCode, out var route))
                    {
                        throw new InvalidOperationException($"Unknown route code {routeCode}.");
                    }

                    customer.Name = GetRequiredValue(row, "Name");
                    customer.OutletCode = GetRequiredValue(row, "OutletCode");
                    customer.ContactPerson = GetOptionalValue(row, "ContactPerson");
                    customer.Phone = GetOptionalValue(row, "Phone");
                    customer.City = GetOptionalValue(row, "City");
                    customer.State = GetOptionalValue(row, "State");
                    customer.Locality = GetOptionalValue(row, "Locality");
                    customer.AddressLine1 = GetOptionalValue(row, "AddressLine1");
                    customer.AddressLine2 = GetOptionalValue(row, "AddressLine2");
                    customer.Pincode = GetOptionalValue(row, "Pincode");
                    customer.GstNumber = GetOptionalValue(row, "GstNumber");
                    customer.OutletType = GetOptionalValue(row, "OutletType");
                    customer.RouteId = route.Id;
                    customer.TerritoryId = route.TerritoryId;
                    customer.DistributorId = route.DistributorId;
                    customer.Latitude = ParseNullableDecimal(GetOptionalValue(row, "Latitude"), row.RowNumber, "Latitude");
                    customer.Longitude = ParseNullableDecimal(GetOptionalValue(row, "Longitude"), row.RowNumber, "Longitude");
                    customer.IsActive = ParseBoolOrDefault(GetOptionalValue(row, "IsActive"), true, row.RowNumber, "IsActive");
                    customer.UpdatedAtUtc = DateTime.UtcNow;
                }
                catch (InvalidOperationException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
                catch (FormatException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
            }

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return validationErrors;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return validationErrors;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<UploadValidationError>> CommitRoutesAsync(List<UploadRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Routes upload has no data rows.");
        }

        var validationErrors = new List<UploadValidationError>();

        var territoryCodes = rows.Select(x => GetRequiredValue(x, "TerritoryCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var distributorCodes = rows.Select(x => GetRequiredValue(x, "DistributorCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var routeCodes = rows.Select(x => GetRequiredValue(x, "RouteCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var territories = await _dbContext.Territories.Where(x => territoryCodes.Contains(x.TerritoryCode)).ToListAsync(cancellationToken);
        var distributors = await _dbContext.Distributors.Where(x => distributorCodes.Contains(x.DistributorCode)).ToListAsync(cancellationToken);
        var existingRoutes = await _dbContext.SalesRoutes.Where(x => routeCodes.Contains(x.RouteCode)).ToListAsync(cancellationToken);

        var territoryMap = territories.ToDictionary(x => x.TerritoryCode, StringComparer.OrdinalIgnoreCase);
        var distributorMap = distributors.ToDictionary(x => x.DistributorCode, StringComparer.OrdinalIgnoreCase);
        var routeMap = existingRoutes.ToDictionary(x => x.RouteCode, StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    var routeCode = GetRequiredValue(row, "RouteCode");
                    if (!routeMap.TryGetValue(routeCode, out var route))
                    {
                        route = new SalesRoute
                        {
                            RouteCode = routeCode
                        };
                        routeMap[routeCode] = route;
                        _dbContext.SalesRoutes.Add(route);
                    }

                    var territoryCode = GetRequiredValue(row, "TerritoryCode");
                    var distributorCode = GetRequiredValue(row, "DistributorCode");

                    if (!territoryMap.TryGetValue(territoryCode, out var territory))
                    {
                        throw new InvalidOperationException($"Unknown territory code {territoryCode}.");
                    }

                    if (!distributorMap.TryGetValue(distributorCode, out var distributor))
                    {
                        throw new InvalidOperationException($"Unknown distributor code {distributorCode}.");
                    }

                    route.RouteName = GetRequiredValue(row, "RouteName");
                    route.TerritoryId = territory.Id;
                    route.DistributorId = distributor.Id;
                    route.IsActive = ParseBoolOrDefault(GetOptionalValue(row, "IsActive"), true, row.RowNumber, "IsActive");
                }
                catch (InvalidOperationException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
                catch (FormatException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
            }

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return validationErrors;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return validationErrors;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<UploadValidationError>> CommitProductsAsync(List<UploadRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Products upload has no data rows.");
        }

        var validationErrors = new List<UploadValidationError>();

        var productCodes = rows.Select(x => GetRequiredValue(x, "ProductCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingProducts = await _dbContext.Products.Where(x => productCodes.Contains(x.ProductCode)).ToListAsync(cancellationToken);
        var productMap = existingProducts.ToDictionary(x => x.ProductCode, StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    var productCode = GetRequiredValue(row, "ProductCode");
                    if (!productMap.TryGetValue(productCode, out var product))
                    {
                        product = new Product
                        {
                            ProductCode = productCode,
                            CreatedAtUtc = DateTime.UtcNow
                        };
                        productMap[productCode] = product;
                        _dbContext.Products.Add(product);
                    }

                    product.Sku = GetRequiredValue(row, "Sku");
                    product.Name = GetRequiredValue(row, "Name");
                    product.Uom = GetOptionalValue(row, "Uom");
                    product.UnitPrice = ParseRequiredDecimal(GetRequiredValue(row, "UnitPrice"), row.RowNumber, "UnitPrice");
                    product.Mrp = ParseNullableDecimal(GetOptionalValue(row, "Mrp"), row.RowNumber, "Mrp");
                    product.IsActive = ParseBoolOrDefault(GetOptionalValue(row, "IsActive"), true, row.RowNumber, "IsActive");
                    product.UpdatedAtUtc = DateTime.UtcNow;
                }
                catch (InvalidOperationException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
                catch (FormatException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
            }

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return validationErrors;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return validationErrors;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<UploadValidationError>> CommitPriceListAsync(List<UploadRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("PriceList upload has no data rows.");
        }

        var validationErrors = new List<UploadValidationError>();

        var productCodes = rows.Select(x => GetRequiredValue(x, "ProductCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingProducts = await _dbContext.Products.Where(x => productCodes.Contains(x.ProductCode)).ToListAsync(cancellationToken);
        var productMap = existingProducts.ToDictionary(x => x.ProductCode, StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    var productCode = GetRequiredValue(row, "ProductCode");
                    if (!productMap.TryGetValue(productCode, out var product))
                    {
                        product = new Product
                        {
                            ProductCode = productCode,
                            Sku = GetOptionalValue(row, "Sku") ?? productCode,
                            Name = GetOptionalValue(row, "Name") ?? productCode,
                            UnitPrice = 0,
                            IsActive = true,
                            CreatedAtUtc = DateTime.UtcNow,
                            UpdatedAtUtc = DateTime.UtcNow
                        };

                        productMap[productCode] = product;
                        _dbContext.Products.Add(product);
                    }

                    product.UnitPrice = ParseRequiredDecimal(GetRequiredValue(row, "UnitPrice"), row.RowNumber, "UnitPrice");
                    product.Mrp = ParseNullableDecimal(GetOptionalValue(row, "Mrp"), row.RowNumber, "Mrp");
                    product.UpdatedAtUtc = DateTime.UtcNow;
                }
                catch (InvalidOperationException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
                catch (FormatException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
            }

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return validationErrors;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return validationErrors;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<List<UploadValidationError>> CommitRouteAssignmentsAsync(List<UploadRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("RouteAssignments upload has no data rows.");
        }

        var validationErrors = new List<UploadValidationError>();

        var routeCodes = rows.Select(x => GetRequiredValue(x, "RouteCode")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var repEmails = rows.Select(x => GetRequiredValue(x, "RepEmail")).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var routes = await _dbContext.SalesRoutes.Where(x => routeCodes.Contains(x.RouteCode)).ToListAsync(cancellationToken);
        var users = await _dbContext.Users
            .Where(x => x.NormalizedEmail != null && repEmails.Select(y => y.ToUpperInvariant()).Contains(x.NormalizedEmail))
            .ToListAsync(cancellationToken);

        var routeMap = routes.ToDictionary(x => x.RouteCode, StringComparer.OrdinalIgnoreCase);
        var userMap = users.ToDictionary(x => x.Email ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        var routeIds = routes.Select(x => x.Id).ToList();
        var repUserIds = users.Select(x => x.Id).ToList();

        var existingAssignments = await _dbContext.RouteAssignments
            .Where(x => routeIds.Contains(x.RouteId) && repUserIds.Contains(x.RepUserId))
            .ToListAsync(cancellationToken);

        var assignmentMap = existingAssignments.ToDictionary(
            x => $"{x.RouteId}:{x.RepUserId}",
            StringComparer.OrdinalIgnoreCase);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in rows)
            {
                try
                {
                    var routeCode = GetRequiredValue(row, "RouteCode");
                    var repEmail = GetRequiredValue(row, "RepEmail");

                    if (!routeMap.TryGetValue(routeCode, out var route))
                    {
                        throw new InvalidOperationException($"Unknown route code {routeCode}.");
                    }

                    if (!userMap.TryGetValue(repEmail, out var user))
                    {
                        throw new InvalidOperationException($"Unknown sales rep email {repEmail}.");
                    }

                    var key = $"{route.Id}:{user.Id}";

                    if (!assignmentMap.TryGetValue(key, out var assignment))
                    {
                        assignment = new RouteAssignment
                        {
                            RouteId = route.Id,
                            RepUserId = user.Id
                        };

                        assignmentMap[key] = assignment;
                        _dbContext.RouteAssignments.Add(assignment);
                    }

                    assignment.StartDate = ParseDateOnlyOrDefault(GetOptionalValue(row, "StartDate"), DateOnly.FromDateTime(DateTime.UtcNow), row.RowNumber, "StartDate");
                    assignment.EndDate = ParseNullableDateOnly(GetOptionalValue(row, "EndDate"), row.RowNumber, "EndDate");
                    assignment.IsActive = assignment.EndDate is null;
                }
                catch (InvalidOperationException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
                catch (FormatException ex)
                {
                    validationErrors.Add(CreateValidationError(row, ex.Message));
                }
            }

            if (validationErrors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return validationErrors;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return validationErrors;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static UploadValidationError CreateValidationError(UploadRow row, string message)
    {
        var rawValues = string.Join(" | ", row.Values.Select(x => $"{x.Key}={x.Value}"));
        return new UploadValidationError(row.RowNumber, message, rawValues);
    }

    private async Task MarkJobFailedAsync(int jobId, List<UploadValidationError> validationErrors, CancellationToken cancellationToken)
    {
        var job = await _dbContext.UploadJobs.FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);
        if (job is null)
        {
            return;
        }

        job.Status = UploadJobStatus.Failed;
        job.ErrorFilePath = await WriteErrorCsvFileAsync(jobId, validationErrors, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkJobFailedAsync(int jobId, string errorDetails, CancellationToken cancellationToken)
    {
        var job = await _dbContext.UploadJobs.FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);
        if (job is null)
        {
            return;
        }

        job.Status = UploadJobStatus.Failed;
        job.ErrorFilePath = await WriteErrorFileAsync(jobId, errorDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> WriteErrorFileAsync(int jobId, string errorDetails, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "upload-errors");
        Directory.CreateDirectory(directory);

        var fileName = $"{jobId}_error.txt";
        var fullPath = Path.Combine(directory, fileName);

        await File.WriteAllTextAsync(fullPath, errorDetails, cancellationToken);

        return Path.Combine("App_Data", "upload-errors", fileName).Replace('\\', '/');
    }

    private async Task<string> WriteErrorCsvFileAsync(int jobId, List<UploadValidationError> validationErrors, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "upload-errors");
        Directory.CreateDirectory(directory);

        var fileName = $"{jobId}_errors.csv";
        var fullPath = Path.Combine(directory, fileName);

        var lines = new List<string>
        {
            "RowNumber,Message,RawValues"
        };

        lines.AddRange(validationErrors.Select(x =>
            string.Join(",",
                x.RowNumber.ToString(CultureInfo.InvariantCulture),
                EscapeCsvValue(x.Message),
                EscapeCsvValue(x.RawValues))));

        await File.WriteAllLinesAsync(fullPath, lines, cancellationToken);

        return Path.Combine("App_Data", "upload-errors", fileName).Replace('\\', '/');
    }

    private static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private string? ResolveUploadFilePath(int jobId)
    {
        var directory = GetUploadDirectoryPath();
        if (!Directory.Exists(directory))
        {
            return null;
        }

        return Directory.GetFiles(directory, $"{jobId}_*")
            .OrderByDescending(File.GetCreationTimeUtc)
            .FirstOrDefault();
    }

    private string BuildUploadFilePath(int jobId, string normalizedFileName)
    {
        return Path.Combine(GetUploadDirectoryPath(), $"{jobId}_{normalizedFileName}");
    }

    private string GetUploadDirectoryPath()
    {
        return Path.Combine(_environment.ContentRootPath, "App_Data", "uploads");
    }

    private static string GetRequiredValue(UploadRow row, string columnName)
    {
        if (!row.Values.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Row {row.RowNumber} is missing required column {columnName}.");
        }

        return value.Trim();
    }

    private static string? GetOptionalValue(UploadRow row, string columnName)
    {
        if (!row.Values.TryGetValue(columnName, out var value))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal ParseRequiredDecimal(string value, int rowNumber, string columnName)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            throw new FormatException($"Row {rowNumber} has invalid decimal value in {columnName}.");
        }

        return parsedValue;
    }

    private static decimal? ParseNullableDecimal(string? value, int rowNumber, string columnName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            throw new FormatException($"Row {rowNumber} has invalid decimal value in {columnName}.");
        }

        return parsedValue;
    }

    private static bool ParseBoolOrDefault(string? value, bool defaultValue, int rowNumber, string columnName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!bool.TryParse(value, out var parsedValue))
        {
            throw new FormatException($"Row {rowNumber} has invalid boolean value in {columnName}.");
        }

        return parsedValue;
    }

    private static DateOnly ParseDateOnlyOrDefault(string? value, DateOnly defaultValue, int rowNumber, string columnName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedValue))
        {
            throw new FormatException($"Row {rowNumber} has invalid date value in {columnName}. Expected yyyy-MM-dd.");
        }

        return parsedValue;
    }

    private static DateOnly? ParseNullableDateOnly(string? value, int rowNumber, string columnName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedValue))
        {
            throw new FormatException($"Row {rowNumber} has invalid date value in {columnName}. Expected yyyy-MM-dd.");
        }

        return parsedValue;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Add('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(new string([.. current]));
                current.Clear();
                continue;
            }

            current.Add(ch);
        }

        values.Add(new string([.. current]));
        return values;
    }

    private sealed record UploadRow(int RowNumber, Dictionary<string, string> Values);
    private sealed record UploadValidationError(int RowNumber, string Message, string RawValues);
}
