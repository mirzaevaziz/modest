using Microsoft.Extensions.Logging;
using Modest.Core.Common.Models;
using Modest.Core.Features.Utils.SequenceNumber;
using Modest.Core.Helpers;

namespace Modest.Core.Features.References.Product;

public interface IProductService
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
    Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto);
    Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto);
    Task<bool> DeleteProductAsync(Guid id);
}

public class ProductService(
    IProductRepository productRepository,
    ISequenceNumberService sequenceNumberService,
    IServiceProvider serviceProvider,
    ILogger<ProductService> logger
) : IProductService
{
    public async Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        return await productRepository.GetAllProductsAsync(request, sortFields);
    }

    public async Task<PaginatedResponse<ProductLookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return await productRepository.GetProductLookupDtosAsync(request);
    }

    public async Task<PaginatedResponse<string>> GetManufacturerLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return await productRepository.GetManufacturerLookupDtosAsync(request);
    }

    public async Task<PaginatedResponse<string>> GetCountryLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return await productRepository.GetCountryLookupDtosAsync(request);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        return await productRepository.GetProductByIdAsync(id);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto)
    {
        ProductServiceLog.CreatingProduct(
            logger,
            productCreateDto.Name,
            productCreateDto.Manufacturer,
            productCreateDto.Country,
            null
        );
        ValidationHelper.ValidateAndThrow(productCreateDto, serviceProvider);

        // Generate product code using sequence service
        var sequenceNumber = await sequenceNumberService.GetNextAsync("products");
        var code = $"SKU-{sequenceNumber:D6}"; // Format: SKU-000001

        var entity = await productRepository.CreateProductAsync(productCreateDto, code);

        ProductServiceLog.ProductCreated(logger, entity.Id, entity.FullName!, null);
        return entity;
    }

    public async Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto)
    {
        ProductServiceLog.UpdatingProduct(
            logger,
            productUpdateDto.Id,
            productUpdateDto.Name,
            productUpdateDto.Manufacturer,
            productUpdateDto.Country,
            null
        );
        ValidationHelper.ValidateAndThrow(productUpdateDto, serviceProvider);

        var entity = await productRepository.UpdateProductAsync(productUpdateDto);

        ProductServiceLog.ProductUpdated(logger, entity.Id, entity.FullName!, null);
        return entity;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        ProductServiceLog.DeletingProduct(logger, id, null);

        var result = await productRepository.DeleteProductAsync(id);
        if (result)
        {
            ProductServiceLog.ProductDeleted(logger, id, null);
        }
        else
        {
            ProductServiceLog.ProductDeleteFailed(logger, id, null);
        }

        return result;
    }
}
