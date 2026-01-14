# Automated Image Validation System

A C#/.NET 8 console application that validates product images for a list of SKUs. For each SKU, the app fetches associated image URLs via a BigCommerce-like client (fake implementation included), downloads images, extracts metadata using a Node.js helper (Sharp), evaluates against quality criteria, and generates CSV reports.

## Features

- Reads SKUs from an input CSV file (expects a header column named `SKU`)
- Fetches image URLs per SKU via `IBigCommerceClient` (fake client provided for deterministic testing)
- Downloads each image and extracts metadata using Node.js + Sharp (width, height, density/DPI when available)
- Validates each image against configurable thresholds (min width/height; optional DPI enforcement)
- Produces two reports:
  - Image-level report: validation per image
  - SKU-level report: rollup per SKU (flags SKUs with any failing images or no images)
- Bounded concurrency for faster processing

## Prerequisites

- .NET 8 SDK
- Node.js + npm (Node 18+ recommended)

## Setup

From the repository root:

1. Restore .NET packages:

   `dotnet restore`

2. Install Node dependencies for the metadata tool:

   `cd tools/image-meta`

   `npm install`
   
   `cd ../..`

### Windows PowerShell note (npm execution policy)

If PowerShell blocks `npm.ps1`, run npm via the cmd shim:

```ps
& "C:\Program Files\nodejs\npm.cmd" install
```

Alternatively, run `npm install` from Command Prompt.

## Configuration

Configuration is stored in `src/ImageValidation.Cli/appsettings.json`:

- `Validation.MinWidthPx`: minimum allowed width (px)
- `Validation.MinHeightPx`: minimum allowed height (px)
- `Validation.MinDpi`: minimum allowed DPI (if enforced)
- `Validation.FailIfDpiMissing`: if true, missing DPI is flagged; otherwise DPI is treated as `N/A`
- `Concurrency.MaxConcurrency`: max number of SKUs processed concurrently
- `Node.ToolsDirectory`: path to the Node helper directory (repo-root-relative)

## Usage

Run with defaults (uses `samples/input-skus.csv` and writes to `output/`):

```ps
dotnet run --project .\src\ImageValidation.Cli\ImageValidation.Cli.csproj
```

Run with custom input/output:

```ps
dotnet run --project .\src\ImageValidation.Cli\ImageValidation.Cli.csproj -- <inputCsv> <outputImageReportCsv>
```

Example:

```ps
dotnet run --project .\src\ImageValidation.Cli\ImageValidation.Cli.csproj -- .\samples\input-skus.csv .\output\output-report.csv
```

## Output

The app writes:

1. Image-level report (per-image)

    - Default: `output/output-report.csv`
    - Columns: `Sku`, `ImageUrl`, `Width`, `Height`, `Dpi`, `DimensionResult`, `DpiResult`, `Status`, `Notes`

2. SKU-level summary report (rollup)

    - Default: `output/output-report-skus.csv`
    - Columns: `Sku`, `ImageCount`, `OkCount`, `FlagCount`, `Status`, `Notes`

SKU status logic:

- `FLAG` if any image is flagged OR the SKU has no images
- otherwise `OK`

## Project Structure

* `src/`

  * `ImageValidation.Cli/` (Main .NET console application)

    * `Models/` (Data models: `Sku`, `Image`, `Metadata`, `ReportRow`, `SkuSummaryRow`)
    * `BigCommerce/` (`IBigCommerceClient` + `FakeBigCommerceClient` + placeholder real client)
    * `Services/` (CSV, download, metadata extraction, validation, reporting, SKU summary)
    * `Utilities/` (Process runner, temp file helpers, concurrency helpers)
    * `appsettings.json` (Configuration)
    * `Program.cs` (Entry point)
* `tools/`

  * `image-meta/` (Node metadata extractor using Sharp)

    * `package.json`
    * `image-meta.js`
* `samples/`

  * `input-skus.csv` (Sample input file)
* `output/` (Output report directory)
* `README.md`

## How it Works

1. Read SKUs from the CSV input file
2. For each SKU:

   * Fetch image URLs from `IBigCommerceClient`
   * Download images to temporary files
   * Invoke `node image-meta.js "<tempPath>"` to extract metadata as JSON
   * Validate against configured thresholds
3. Write the image-level report CSV
4. Build and write the SKU-level summary CSV
5. Print a short console summary

## Swapping FakeBigCommerceClient for a Real BigCommerce Client

1. Implement `IBigCommerceClient` using `HttpClient` (e.g., `RealBigCommerceClient`) to call BigCommerce endpoints:

   * Look up product by SKU (catalog/products)
   * Retrieve product images (catalog/products/{productId}/images)

2. Replace the client instantiation in `Program.cs`:

   ```cs
   // var bigCommerceClient = new FakeBigCommerceClient();
   var bigCommerceClient = new RealBigCommerceClient(httpClient, /* credentials/config */);
   ```
3. Provide credentials via configuration or environment variables for real integrations.

### Additional Considerations for Real BigCommerce Implementation

When implementing a real BigCommerce client, consider these complexities:

1. **SKU Mapping Complexity**: In real BigCommerce implementations, SKU mapping can be complex because:
   - SKUs can exist at both product and variant levels
   - A single SKU might map to multiple products/variants
   - The API might require different lookup paths depending on store configuration

2. **API Response Structure**: The BigCommerce API returns rich metadata with images that could be leveraged:
   - Image descriptions/alt text for accessibility validation
   - Image position/order for display validation
   - Image file names for naming convention validation

3. **Performance Considerations**: 
   - Add caching for product lookups to avoid repeated API calls
   - Implement bulk operations if processing many SKUs
   - Handle rate limiting (429 responses) with appropriate backoff strategies

4. **Error Handling**:
   - Handle authentication errors (401, 403)
   - Implement retry logic for transient failures
   - Log API errors for debugging and monitoring

## Sample Input

`samples/input-skus.csv` includes:

* SKU001: multiple images (mixed results)
* SKU002: single passing image
* SKU003: no images
* SKU004: multiple images (mixed results)
* SKU005: unknown SKU (no images)
