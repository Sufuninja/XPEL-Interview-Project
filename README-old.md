# Automated Image Validation System

This is a C#/.NET console application that validates product images by fetching them from a BigCommerce-like API, analyzing their properties using a Node.js helper with the Sharp library, and generating a compliance report.

## Features

- Parses product SKUs from a CSV input file
- Fetches product images from a BigCommerce API (using a fake client for testing)
- Extracts image metadata (width, height, DPI) using Node.js and Sharp
- Validates images against configurable quality criteria
- Generates a detailed CSV report with validation results
- Supports concurrent processing for improved performance

## Prerequisites

- .NET 8 SDK
- Node.js (version 14 or higher)
- npm (comes with Node.js)

## Setup

1. **Install .NET dependencies:**
   ```bash
   dotnet restore
   ```

2. **Install Node.js dependencies:**
   ```bash
   cd tools/image-meta
   npm install
   ```

## Configuration

The application uses the following configuration settings in `src/ImageValidation.Cli/appsettings.json`:

- `Validation.MinWidthPx`: Minimum required image width in pixels
- `Validation.MinHeightPx`: Minimum required image height in pixels
- `Validation.MinDpi`: Minimum required DPI (dots per inch)
- `Validation.FailIfDpiMissing`: Whether to flag images when DPI information is missing
- `Concurrency.MaxConcurrency`: Maximum number of concurrent image processing operations

## Usage

Run the application with default input/output files:
```bash
dotnet run
```

Or specify custom input and output CSV files:
```bash
dotnet run -- <inputCsv> <outputCsv>
```

Example:
```bash
dotnet run -- samples/input-skus.csv output/report.csv
```

## Project Structure

```
├── src/
│   └── ImageValidation.Cli/     # Main C# console application
│       ├── Models/              # Data models (Sku, Image, Metadata, ReportRow)
│       ├── BigCommerce/         # BigCommerce client interface and implementations
│       ├── Services/            # Core services (CSV, download, validation, reporting)
│       ├── Utilities/           # Helper classes (ProcessRunner, TempFileHelper, ConcurrencyHelper)
│       ├── appsettings.json     # Configuration file
│       └── Program.cs           # Main entry point
├── tools/
│   └── image-meta/              # Node.js helper for image metadata extraction
│       ├── package.json         # Node.js dependencies
│       └── image-meta.js        # Image metadata extraction script
├── samples/                     # Sample data
│   └── input-skus.csv           # Sample input CSV file
├── output/                      # Output directory for reports
└── README.md                    # This file
```

## Swapping FakeBigCommerceClient for a Real Implementation

To use a real BigCommerce API client instead of the fake one:

1. Implement the `IBigCommerceClient` interface in `src/ImageValidation.Cli/BigCommerce/RealBigCommerceClient.cs`
2. Modify the service creation in `Program.cs`:
   ```csharp
   // Replace this line:
   var bigCommerceClient = new FakeBigCommerceClient();
   
   // With:
   var bigCommerceClient = new RealBigCommerceClient();
   ```

## How It Works

1. The application reads SKUs from the input CSV file
2. For each SKU, it fetches associated product images from the BigCommerce API
3. Each image is downloaded to a temporary file
4. The Node.js helper script (`image-meta.js`) is invoked to extract image metadata using the Sharp library
5. Images are validated against the configured quality criteria
6. A detailed CSV report is generated with validation results for each image
7. Summary statistics are printed to the console

## Report Format

The output CSV contains the following columns:
- `Sku`: Product SKU
- `ImageUrl`: URL of the image
- `Width`: Image width in pixels
- `Height`: Image height in pixels
- `Dpi`: Image DPI (if available)
- `DimensionResult`: "PASS", "FAIL", or "ERROR"
- `DpiResult`: "PASS", "FAIL", "N/A", or "ERROR"
- `Status`: "OK" or "FLAG"
- `Notes`: Additional information about validation results or errors

## Sample Data

The `samples/input-skus.csv` file contains sample SKUs for testing:
- SKU001: Multiple images (one passing, one failing)
- SKU002: Single passing image
- SKU003: No images
- SKU004: Multiple images with mixed results
- SKU005: Unknown SKU (no images)