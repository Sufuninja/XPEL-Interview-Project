namespace ImageValidation.Cli.Models;

public record Metadata(
    int? Width,
    int? Height,
    double? Density,
    string Format);