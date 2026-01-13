using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.BigCommerce;

public class FakeBigCommerceClient : IBigCommerceClient
{
    private readonly Dictionary<string, List<string>> _skuImages = new()
    {
        // SKU with multiple images (one passing, one failing)
        { "SKU001", new() { 
            "https://picsum.photos/800/600",  // Passing image
            "https://picsum.photos/200/150"   // Failing image
        }},
        
        // SKU with a single passing image
        { "SKU002", new() { 
            "https://picsum.photos/1200/800"  
        }},
        
        // SKU with no images
        { "SKU003", new() },
        
        // Another SKU with mixed results
        { "SKU004", new() { 
            "https://picsum.photos/1000/750", // Passing
            "https://picsum.photos/300/200",  // Failing
            "https://picsum.photos/1500/1000" // Passing
        }}
    };

    public async Task<IEnumerable<Image>> GetProductImagesAsync(Sku sku)
    {
        await Task.Delay(10); // Simulate network delay
        
        if (_skuImages.TryGetValue(sku.Value, out var imageUrls))
        {
            return imageUrls.Select(url => new Image(url));
        }
        
        // Return empty list for unknown SKUs
        return Enumerable.Empty<Image>();
    }
}