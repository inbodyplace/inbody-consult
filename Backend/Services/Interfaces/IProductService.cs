using Backend.Models;

namespace Backend.Services;

public interface IProductService
{
    Task<ProductDetailResponse?> GetProductAsync(int productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductListItemResponse>> GetProductsAsync(
        CancellationToken cancellationToken
    );
    Task<ProductResponse> CreateProductAsync(string name, CancellationToken cancellationToken);
}
