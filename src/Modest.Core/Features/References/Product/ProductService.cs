using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modest.Core.Common.Models;
using Modest.Core.Data;
using Modest.Core.Helpers;
using MongoDB.Driver;

namespace Modest.Core.Features.References.Product;

public interface IProductService
{
    Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    );
    Task<PaginatedResponse<LookupDto>> GetProductLookupDtosAsync(PaginatedRequest<string> request);
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
    ModestDbContext modestDbContext,
    IServiceProvider serviceProvider,
    ILogger<ProductService> logger
) : IProductService
{
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var entity = await modestDbContext.Products.FindAsync(id);
        if (entity == null || entity.IsDeleted)
        {
            return null;
        }

        return entity.ToProductDto();
    }

    public async Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        var query = modestDbContext.Products.AsQueryable();
        if (request.Filter is not null)
        {
            if (request.Filter.ShowDeleted.HasValue)
            {
                query = query.Where(w => w.IsDeleted == !request.Filter.ShowDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter.SearchText))
            {
                query = query.Where(w => w.FullName.Contains(request.Filter.SearchText));
            }

            if (!string.IsNullOrWhiteSpace(request.Filter.Manufacturer))
            {
                query = query.Where(w => w.Manufacturer == request.Filter.Manufacturer);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter.Country))
            {
                query = query.Where(w => w.Country == request.Filter.Country);
            }
        }

        //TODO: Need To receive sort field and direction
        query = query.OrderBy(o => o.FullName);

        return await PaginationHelper.PaginateAsync(
            query,
            s => new ProductDto(
                s.Id,
                s.IsDeleted,
                s.CreatedAt,
                s.UpdatedAt,
                s.DeletedAt,
                s.FullName,
                s.Name,
                s.Manufacturer,
                s.Country
            ),
            request.PageNumber,
            request.PageSize,
            sortFields ?? [new SortFieldRequest("FullName", true)]
        );
    }

    public async Task<PaginatedResponse<LookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var query = modestDbContext.Products.Where(w => w.IsDeleted == false);
        if (!string.IsNullOrEmpty(request.Filter))
        {
            query = query.Where(w => w.FullName.Contains(request.Filter));
        }

        return await PaginationHelper.PaginateAsync(
            query,
            s => new LookupDto(s.Id, s.FullName),
            request.PageNumber,
            request.PageSize,
            [new SortFieldRequest("FullName", true)]
        );
    }

    public async Task<PaginatedResponse<string>> GetManufacturerLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var query = modestDbContext.Products.Where(w =>
            w.IsDeleted == false && w.Manufacturer != null && w.Manufacturer.Length > 0
        );
        if (!string.IsNullOrEmpty(request.Filter))
        {
            query = query.Where(w => w.Manufacturer!.Contains(request.Filter));
        }

        // Select only manufacturer names and make them distinct
        var distinctQuery = query.Select(w => w.Manufacturer!).Distinct().OrderBy(o => o);

        return await PaginationHelper.PaginateAsync(
            distinctQuery,
            s => s,
            request.PageNumber,
            request.PageSize,
            [] // No sorting field for simple string list
        );
    }

    public async Task<PaginatedResponse<string>> GetCountryLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var query = modestDbContext.Products.Where(w =>
            w.IsDeleted == false && w.Country != null && w.Country.Length > 0
        );
        if (!string.IsNullOrEmpty(request.Filter))
        {
            query = query.Where(w => w.Country!.Contains(request.Filter));
        }

        // Select only Country names and make them distinct
        var distinctQuery = query.Select(w => w.Country!).Distinct().OrderBy(o => o);

        return await PaginationHelper.PaginateAsync(
            distinctQuery,
            s => s,
            request.PageNumber,
            request.PageSize,
            [] // No sorting field for simple string list
        );
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

        var entity = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = productCreateDto.Name,
            Manufacturer = productCreateDto.Manufacturer,
            Country = productCreateDto.Country,
        };
        var duplicate = await modestDbContext.Products.FirstOrDefaultAsync(p =>
            p.FullName == entity.FullName
        );
        if (duplicate is not null)
        {
            if (duplicate.IsDeleted)
            {
                duplicate.IsDeleted = false;
                entity = duplicate;
                ProductServiceLog.UpdatedDuplicatedProduct(
                    logger,
                    duplicate.Id,
                    duplicate.FullName,
                    null
                );
            }
            else
            {
                throw new ValidationException($"Product with the same FullName already exists.");
            }
        }
        else
        {
            modestDbContext.Products.Add(entity);
        }

        await modestDbContext.SaveChangesAsync();

        ProductServiceLog.ProductCreated(logger, entity.Id, entity.FullName, null);
        return entity.ToProductDto();
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

        var entity = await modestDbContext.Products.FindAsync(productUpdateDto.Id);
        if (entity == null || entity.IsDeleted)
        {
            throw new ProductNotFoundException(productUpdateDto.Id);
        }

        entity.Name = productUpdateDto.Name;
        entity.Manufacturer = productUpdateDto.Manufacturer;
        entity.Country = productUpdateDto.Country;

        var duplicate = await modestDbContext.Products.FirstOrDefaultAsync(p =>
            p.FullName == entity.FullName
        );
        if (duplicate is not null && duplicate.Id != entity.Id)
        {
            if (duplicate.IsDeleted)
            {
                duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
                ProductServiceLog.UpdatedDuplicatedProduct(
                    logger,
                    duplicate.Id,
                    duplicate.FullName,
                    null
                );
            }
            else
            {
                throw new ValidationException($"Product with the same FullName already exists.");
            }
        }

        await modestDbContext.SaveChangesAsync();

        ProductServiceLog.ProductUpdated(logger, entity.Id, entity.FullName, null);
        return entity.ToProductDto();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        ProductServiceLog.DeletingProduct(logger, id, null);
        var entity = await modestDbContext.Products.FindAsync(id);
        if (entity == null || entity.IsDeleted)
        {
            return false;
        }

        modestDbContext.Products.Remove(entity);
        var result = await modestDbContext.SaveChangesAsync() > 0;
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
