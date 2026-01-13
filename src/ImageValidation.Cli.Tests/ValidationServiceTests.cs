using ImageValidation.Cli.Models;
using ImageValidation.Cli.Services;
using Xunit;

namespace ImageValidation.Cli.Tests;

public class ValidationServiceTests
{
    [Fact]
    public void ValidateImage_Passes_When_Dimensions_Meet_Minimum_Requirements()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0);
        var metadata = new Metadata(600, 600, 72.0, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("PASS", dimensionResult);
        Assert.Equal("PASS", dpiResult);
        Assert.Equal("OK", status);
    }

    [Fact]
    public void ValidateImage_Fails_When_Width_Below_Minimum()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0);
        var metadata = new Metadata(400, 600, 72.0, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("FAIL", dimensionResult);
        Assert.Equal("FLAG", status);
        Assert.Contains("below minimum", notes);
    }

    [Fact]
    public void ValidateImage_Fails_When_Height_Below_Minimum()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0);
        var metadata = new Metadata(600, 400, 72.0, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("FAIL", dimensionResult);
        Assert.Equal("FLAG", status);
        Assert.Contains("below minimum", notes);
    }

    [Fact]
    public void ValidateImage_Fails_When_DPI_Below_Minimum()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0);
        var metadata = new Metadata(600, 600, 60.0, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("PASS", dimensionResult);
        Assert.Equal("FAIL", dpiResult);
        Assert.Equal("FLAG", status);
        Assert.Contains("DPI", notes);
    }

    [Fact]
    public void ValidateImage_Passes_When_DPI_Missing_And_FailIfDpiMissing_Is_False()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0, false);
        var metadata = new Metadata(600, 600, null, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("PASS", dimensionResult);
        Assert.Equal("N/A", dpiResult);
        Assert.Equal("OK", status);
        Assert.Contains("not available", notes);
    }

    [Fact]
    public void ValidateImage_Fails_When_DPI_Missing_And_FailIfDpiMissing_Is_True()
    {
        // Arrange
        var validationService = new ValidationService(500, 500, 72.0, true);
        var metadata = new Metadata(600, 600, null, "jpeg");

        // Act
        var (dimensionResult, dpiResult, status, notes) = validationService.ValidateImage(metadata);

        // Assert
        Assert.Equal("PASS", dimensionResult);
        Assert.Equal("FAIL", dpiResult);
        Assert.Equal("FLAG", status);
        Assert.Contains("missing", notes);
    }
}