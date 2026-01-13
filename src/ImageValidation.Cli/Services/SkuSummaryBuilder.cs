using System.Reflection.Metadata.Ecma335;
using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.Services;

public sealed class SkuSummaryBuilder
{
    public List<SkuSummaryRow> Build(IEnumerable<ReportRow> rows)
    {
        return rows
            .GroupBy(r => r.Sku, StringComparer.OrdinalIgnoreCase)
            .Select(g => {
                var imageRows = g.Where(r => !string.IsNullOrWhiteSpace(r.ImageUrl)).ToList();

                var imageCount = imageRows.Count;
                var flagCount = imageRows.Count(r => r.Status == "FLAG");
                var okCount = imageRows.Count(r => r.Status == "OK");
                var isNoImages = imageRows.Count == 0;
                var status = (isNoImages || flagCount > 0) ? "FLAG" : "OK";

                var notes = isNoImages ? "No images found" : BuildFailureSummary(imageRows);
                return new SkuSummaryRow(
                    Sku: g.Key,
                    ImageCount: imageCount,
                    OkCount: okCount,
                    FlagCount: flagCount,
                    Status: status,
                    Notes: notes
                );
            })
            .OrderBy(r => r.Status == "FLAG" ? 0 : 1)
            .ThenBy(r => r.Sku, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildFailureSummary(List<ReportRow> imageRows)
    {
        var failing = imageRows.Where(r => r.Status == "FLAG").ToList();
        if (failing.Count == 0) return "";
        
        var grouped = failing
        .Where(r => !string.IsNullOrWhiteSpace(r.Notes))
        .GroupBy(r => r.Notes)
        .Select(g => $"{g.Count()} image(s): {g.Key}")
        .ToList();

        if (grouped.Count == 0)
        return $"{failing.Count} image(s) flagged";

        return string.Join("; ", grouped);
    }
}