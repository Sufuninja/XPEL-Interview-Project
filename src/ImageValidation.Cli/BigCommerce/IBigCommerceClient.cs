using ImageValidation.Cli.Models;

namespace ImageValidation.Cli.BigCommerce;

public interface IBigCommerceClient
{
    Task<IEnumerable<Image>> GetProductImagesAsync(Sku sku);
}