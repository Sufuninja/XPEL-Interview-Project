using ImageValidation.Cli.Models;
using ImageValidation.Cli.Utilities;

namespace ImageValidation.Cli.Services;

public class ImageDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly TempFileHelper _tempFileHelper;

    public ImageDownloadService(HttpClient httpClient, TempFileHelper tempFileHelper)
    {
        _httpClient = httpClient;
        _tempFileHelper = tempFileHelper;
    }

    public async Task<string> DownloadImageToTempFileAsync(Image image)
    {
        try
        {
            var response = await _httpClient.GetAsync(image.Url);
            response.EnsureSuccessStatusCode();
            
            var tempFilePath = _tempFileHelper.CreateTempFile(".jpg");
            await using var fileStream = File.Create(tempFilePath);
            await response.Content.CopyToAsync(fileStream);
            
            return tempFilePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download image from {image.Url}: {ex.Message}", ex);
        }
    }
}