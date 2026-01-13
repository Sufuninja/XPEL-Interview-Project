using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.Services;

public class ValidationService
{
    private readonly int _minWidthPx;
    private readonly int _minHeightPx;
    private readonly double _minDpi;
    private readonly bool _failIfDpiMissing;

    public ValidationService(int minWidthPx, int minHeightPx, double minDpi = 72.0, bool failIfDpiMissing = false)
    {
        _minWidthPx = minWidthPx;
        _minHeightPx = minHeightPx;
        _minDpi = minDpi;
        _failIfDpiMissing = failIfDpiMissing;
    }

    public (string DimensionResult, string DpiResult, string Status, string Notes) ValidateImage(Metadata metadata)
    {
        var dimensionResult = "PASS";
        var dpiResult = "PASS";
        var status = "OK";
        var notes = new List<string>();

        // Check dimensions
        if (metadata.Width < _minWidthPx || metadata.Height < _minHeightPx)
        {
            dimensionResult = "FAIL";
            status = "FLAG";
            notes.Add($"Dimensions {metadata.Width}x{metadata.Height} below minimum {_minWidthPx}x{_minHeightPx}");
        }

        // Check DPI
        if (metadata.Density.HasValue)
        {
            if (metadata.Density.Value < _minDpi)
            {
                dpiResult = "FAIL";
                status = "FLAG";
                notes.Add($"DPI {metadata.Density.Value} below minimum {_minDpi}");
            }
        }
        else
        {
            if (_failIfDpiMissing)
            {
                dpiResult = "FAIL";
                status = "FLAG";
                notes.Add("DPI information missing");
            }
            else
            {
                dpiResult = "N/A";
                notes.Add("DPI information not available");
            }
        }

        return (dimensionResult, dpiResult, status, string.Join("; ", notes));
    }
}