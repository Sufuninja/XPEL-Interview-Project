namespace ImageValidation.Cli.Models;

public record ReportRow(
    string Sku,
    string ImageUrl,
    int? Width,
    int? Height,
    double? Dpi,
    string DimensionResult,
    string DpiResult,
    string Status,
    string Notes);