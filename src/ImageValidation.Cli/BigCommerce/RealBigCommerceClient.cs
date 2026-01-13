using System.Net.Http.Headers;
using System.Text.Json;
using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.BigCommerce;

public sealed class RealBigCommerceClient : IBigCommerceClient
{
    private readonly HttpClient _http;
    private readonly BigCommerceOptions _options;

    public RealBigCommerceClient(HttpClient httpClient, BigCommerceOptions options)
    {
        _http = httpClient;
        _options = options;

        // BigCommerce v3 base:
        // https://api.bigcommerce.com/stores/{storeHash}/v3/
        _http.BaseAddress = new Uri($"https://api.bigcommerce.com/stores/{_options.StoreHash}/v3/");

        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.Add("X-Auth-Token", _options.AccessToken);
    }

    public async Task<IEnumerable<Image>> GetProductImagesAsync(Sku sku)
    {
        // NOTE: SKU can represent a product SKU or a variant SKU depending on catalog configuration.
        // This implementation assumes product-level SKU lookup. In many real stores, you may need to
        // search variants (catalog/variants) or map SKU->productId differently.

        var productId = await TryFindProductIdBySkuAsync(sku.Value);
        if (productId is null)
            return Enumerable.Empty<Image>();

        var images = await GetImagesByProductIdAsync(productId.Value);
        return images.Select(u => new Image(u));
    }

    private async Task<long?> TryFindProductIdBySkuAsync(string sku)
    {
        // BigCommerce supports filtering products by sku via query parameters in many configurations.
        // If unsupported in the target environment, this would need to be replaced with another lookup path.
        var url = $"catalog/products?sku={Uri.EscapeDataString(sku)}&limit=1";

        using var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return null;

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<BigCommerceListResponse<BigCommerceProduct>>(json, JsonOptions);
        return parsed?.Data?.FirstOrDefault()?.Id;
    }

    private async Task<List<string>> GetImagesByProductIdAsync(long productId)
    {
        var url = $"catalog/products/{productId}/images";

        using var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return new List<string>();

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<BigCommerceListResponse<BigCommerceImage>>(json, JsonOptions);

        // BigCommerce image payloads often include different URL fields; adjust based on actual response.
        return parsed?.Data?
            .Select(i => i.UrlStandard ?? i.UrlOriginal ?? i.UrlThumbnail)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList() ?? new List<string>();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record BigCommerceListResponse<T>(List<T> Data);

    private sealed record BigCommerceProduct(long Id);

    private sealed record BigCommerceImage(
        string? UrlStandard,
        string? UrlOriginal,
        string? UrlThumbnail
    );
}

public sealed class BigCommerceOptions
{
    public string StoreHash { get; init; } = "";
    public string AccessToken { get; init; } = "";
}
