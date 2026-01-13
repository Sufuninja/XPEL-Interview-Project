using ImageValidation.Cli.BigCommerce;
using ImageValidation.Cli.Models;
using ImageValidation.Cli.Services;
using ImageValidation.Cli.Utilities;
using System.Net.Http.Json;

namespace ImageValidation.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var repoRoot = RepoPath.FindRepoRoot(AppContext.BaseDirectory);

            // If user passes args, respect them; otherwise use repo-root-relative defaults
            var inputCsvPath = args.Length > 0
                ? Path.GetFullPath(args[0])
                : Path.Combine(repoRoot, "samples", "input-skus.csv");

            var outputCsvPath = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(repoRoot, "output", "output-report.csv");

            // Ensure output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputCsvPath)!);

            // Load configuration (now takes repoRoot)
            var configuration = LoadConfiguration(repoRoot);

            // Create services
            using var httpClient = new HttpClient();
            using var tempFileHelper = new TempFileHelper();
            var processRunner = new ProcessRunner();
            var csvService = new CsvService();
            var bigCommerceClient = new FakeBigCommerceClient();
            var imageDownloadService = new ImageDownloadService(httpClient, tempFileHelper);
            var nodeImageMetadataService = new NodeImageMetadataService(processRunner, configuration.Node.ToolsDirectory);
            var validationService = new ValidationService(
                configuration.Validation.MinWidthPx,
                configuration.Validation.MinHeightPx,
                configuration.Validation.MinDpi,
                configuration.Validation.FailIfDpiMissing);
            var reportWriter = new ReportWriter();
            var concurrencyHelper = new ConcurrencyHelper(configuration.Concurrency.MaxConcurrency);
            var skuSummaryBuilder = new SkuSummaryBuilder();

            // Read SKUs from CSV
            var skus = await csvService.ReadSkusFromCsvAsync(inputCsvPath);
            Console.WriteLine($"Processing {skus.Count()} SKUs...");

            // Process each SKU
            var reportRows = new List<ReportRow>();
            var semaphore = new SemaphoreSlim(configuration.Concurrency.MaxConcurrency, configuration.Concurrency.MaxConcurrency);

            var tasks = skus.Select(async sku =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var images = await bigCommerceClient.GetProductImagesAsync(sku);

                    if (!images.Any())
                    {
                        // No images found for this SKU
                        lock (reportRows)
                        {
                            reportRows.Add(reportWriter.CreateNoImagesReportRow(sku.Value));
                        }
                        return;
                    }

                    // Process each image for this SKU
                    foreach (var image in images)
                    {
                        try
                        {
                            // Download image to temp file
                            var tempImagePath = await imageDownloadService.DownloadImageToTempFileAsync(image);

                            // Extract metadata using Node.js helper
                            var metadata = await nodeImageMetadataService.ExtractMetadataAsync(tempImagePath);

                            // Validate image
                            var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

                            // Create report row
                            var reportRow = reportWriter.CreateReportRow(
                                sku.Value,
                                image.Url,
                                metadata,
                                dimensionResult,
                                dpiResult,
                                status,
                                notes);

                            lock (reportRows)
                            {
                                reportRows.Add(reportRow);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Error processing this image
                            var errorReportRow = reportWriter.CreateErrorReportRow(
                                sku.Value,
                                image.Url,
                                ex.Message);

                            lock (reportRows)
                            {
                                reportRows.Add(errorReportRow);
                            }
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            reportRows = reportRows
                .OrderBy(r => r.Sku)
                .ThenBy(r => r.Status == "FLAG" ? 0 : 1)
                .ThenBy(r => r.ImageUrl)
                .ToList();

            // Write report to CSV
            await csvService.WriteReportToCsvAsync(outputCsvPath, reportRows);

            // Build SKU summary
            var skuSummaryPath = Path.Combine(Path.GetDirectoryName(outputCsvPath)!, "output-report-skus.csv");
            var skuSummaryRows = skuSummaryBuilder.Build(reportRows);

            await csvService.WriteReportToCsvAsync(skuSummaryPath, skuSummaryRows);

            // Print summary
            var flaggedCount = reportRows.Count(r => r.Status == "FLAG");
            var okCount = reportRows.Count(r => r.Status == "OK");
            Console.WriteLine("Processing complete!");
            Console.WriteLine($"OK: {okCount} images");
            Console.WriteLine($"FLAGGED: {flaggedCount} images");
            Console.WriteLine($"Report written to {outputCsvPath}");
            Console.WriteLine($"SKU summary written to {skuSummaryPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static Configuration LoadConfiguration(string repoRoot)
    {
        var configuration = new Configuration();

        // Hardcoded defaults (fine for interview), but now stable regardless of CWD.
        configuration.Validation.MinWidthPx = 500;
        configuration.Validation.MinHeightPx = 500;
        configuration.Validation.MinDpi = 72.0;
        configuration.Validation.FailIfDpiMissing = false;

        configuration.Concurrency.MaxConcurrency = 4;

        // Make Node tools directory absolute to repo root
        configuration.Node.ToolsDirectory = Path.Combine(repoRoot, "tools", "image-meta");

        return configuration;
    }

}

class Configuration
{
    public ValidationConfig Validation { get; set; } = new();
    public ConcurrencyConfig Concurrency { get; set; } = new();
    public NodeConfig Node { get; set; } = new();
}

class ValidationConfig
{
    public int MinWidthPx { get; set; }
    public int MinHeightPx { get; set; }
    public double MinDpi { get; set; }
    public bool FailIfDpiMissing { get; set; }
}

class ConcurrencyConfig
{
    public int MaxConcurrency { get; set; }
}

class NodeConfig
{
    public string ToolsDirectory { get; set; } = string.Empty;
}