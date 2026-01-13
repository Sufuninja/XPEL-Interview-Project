namespace ImageValidation.Cli.Models;

public record SkuSummaryRow(
    string Sku,
    int ImageCount,
    int OkCount,
    int FlagCount,
    string Status,
    string Notes
);