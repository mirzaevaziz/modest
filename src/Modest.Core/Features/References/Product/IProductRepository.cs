using Modest.Core.Common.Models;

namespace Modest.Core.Features.References.Product;

public interface IProductRepository
{
    Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    );
    Task<PaginatedResponse<ProductLookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    );
    Task<PaginatedResponse<string>> GetManufacturerLookupDtosAsync(
        PaginatedRequest<string> request
    );
    Task<PaginatedResponse<string>> GetCountryLookupDtosAsync(PaginatedRequest<string> request);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto, string code);
    Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto);
    Task<bool> DeleteProductAsync(Guid id);
}
