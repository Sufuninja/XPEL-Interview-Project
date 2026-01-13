using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.Services;

public class ReportWriter
{
    public ReportRow CreateReportRow(
        string sku,
        string imageUrl,
        Metadata metadata,
        string dimensionResult,
        string dpiResult,
        string status,
        string notes)
    {
        return new ReportRow(
            Sku: sku,
            ImageUrl: imageUrl,
            Width: metadata.Width,
            Height: metadata.Height,
            Dpi: metadata.Density,
            DimensionResult: dimensionResult,
            DpiResult: dpiResult,
            Status: status,
            Notes: notes
        );
    }

    public ReportRow CreateErrorReportRow(
        string sku,
        string imageUrl,
        string error)
    {
        return new ReportRow(
            Sku: sku,
            ImageUrl: imageUrl,
            Width: null,
            Height: null,
            Dpi: null,
            DimensionResult: "ERROR",
            DpiResult: "ERROR",
            Status: "FLAG",
            Notes: error
        );
    }

    public ReportRow CreateNoImagesReportRow(string sku)
    {
        return new ReportRow(
            Sku: sku,
            ImageUrl: "",
            Width: null,
            Height: null,
            Dpi: null,
            DimensionResult: "N/A",
            DpiResult: "N/A",
            Status: "FLAG",
            Notes: "No images found"
        );
    }
}