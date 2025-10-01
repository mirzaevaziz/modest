using FluentValidation;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Modest.Data.Features.References.Product;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductEntity> _collection;

    public ProductRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ProductEntity>("products");

        // Ensure index for FullName (compound index on Name, Manufacturer, Country)
        var indexKeys = Builders<ProductEntity>.IndexKeys.Ascending(x => x.FullName);
        var indexModel = new CreateIndexModel<ProductEntity>(
            indexKeys,
            new CreateIndexOptions { Unique = true, Name = "idx_fullname" }
        );
        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto)
    {
        var entity = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = productCreateDto.Name,
            Manufacturer = productCreateDto.Manufacturer,
            Country = productCreateDto.Country,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };

        var duplicate = await _collection
            .Find(x => x.FullName == entity.FullName)
            .FirstOrDefaultAsync();
        if (duplicate != null)
        {
            if (!duplicate.IsDeleted)
            {
                throw new ValidationException($"Product with the same FullName already exists.");
            }
            // Undo delete (restore)
            duplicate.IsDeleted = false;
            duplicate.DeletedAt = null;
            duplicate.Name = productCreateDto.Name;
            duplicate.Manufacturer = productCreateDto.Manufacturer;
            duplicate.Country = productCreateDto.Country;
            duplicate.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
            return duplicate.ToProductDto();
        }

        await _collection.InsertOneAsync(entity);
        return entity.ToProductDto();
    }

    public async Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto)
    {
        var entity =
            await _collection
                .Find(x => x.Id == productUpdateDto.Id && !x.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new ProductNotFoundException(productUpdateDto.Id);
        entity.Name = productUpdateDto.Name;
        entity.Manufacturer = productUpdateDto.Manufacturer;
        entity.Country = productUpdateDto.Country;
        entity.UpdatedAt = DateTime.UtcNow;
        var duplicate = await _collection
            .Find(x => x.FullName == entity.FullName)
            .FirstOrDefaultAsync();
        if (duplicate is not null && duplicate.Id != entity.Id)
        {
            if (!duplicate.IsDeleted)
            {
                throw new ValidationException($"Product with the same FullName already exists.");
            }
            // Undo delete (restore)
            duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
            await _collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
        }

        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity.ToProductDto();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var entity = await _collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        var filterBuilder = Builders<ProductEntity>.Filter;
        var filter = filterBuilder.Eq(x => x.IsDeleted, false);
        if (request.Filter != null)
        {
            if (!string.IsNullOrEmpty(request.Filter.SearchText))
            {
                filter &= filterBuilder.Regex(
                    x => x.FullName,
                    new BsonRegularExpression(request.Filter.SearchText, "i")
                );
            }

            if (!string.IsNullOrEmpty(request.Filter.Manufacturer))
            {
                filter &= filterBuilder.Eq(x => x.Manufacturer, request.Filter.Manufacturer);
            }

            if (!string.IsNullOrEmpty(request.Filter.Country))
            {
                filter &= filterBuilder.Eq(x => x.Country, request.Filter.Country);
            }
        }

        var query = _collection.Find(filter);
        // Sorting
        if (sortFields != null)
        {
            var sortDef = Builders<ProductEntity>.Sort.Combine(
                sortFields.Select(sf =>
                    sf.Ascending
                        ? Builders<ProductEntity>.Sort.Ascending(sf.FieldName)
                        : Builders<ProductEntity>.Sort.Descending(sf.FieldName)
                )
            );
            query = query.Sort(sortDef);
        }

        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();
        var dtos = items.Select(x => x.ToProductDto()).ToList();
        return new PaginatedResponse<ProductDto>
        {
            Items = dtos,
            TotalCount = (int)total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }

    public async Task<PaginatedResponse<string>> GetCountryLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var countries = await _collection.DistinctAsync<string>(
            "Country",
            Builders<ProductEntity>.Filter.Eq(x => x.IsDeleted, false)
        );
        var list = await countries.ToListAsync();
        if (!string.IsNullOrEmpty(request.Filter))
        {
            list = list.Where(c => c.Contains(request.Filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = list.Count;
        var paged = list.Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return new PaginatedResponse<string>
        {
            Items = paged,
            TotalCount = total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }

    public async Task<PaginatedResponse<string>> GetManufacturerLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var manufacturers = await _collection.DistinctAsync<string>(
            "Manufacturer",
            Builders<ProductEntity>.Filter.Eq(x => x.IsDeleted, false)
        );
        var list = await manufacturers.ToListAsync();
        if (!string.IsNullOrEmpty(request.Filter))
        {
            list = list.Where(m => m.Contains(request.Filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = list.Count;
        var paged = list.Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        return new PaginatedResponse<string>
        {
            Items = paged,
            TotalCount = total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return entity?.ToProductDto();
    }

    public async Task<PaginatedResponse<LookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var filterBuilder = Builders<ProductEntity>.Filter;
        var filter = filterBuilder.Eq(x => x.IsDeleted, false);
        if (!string.IsNullOrEmpty(request.Filter))
        {
            filter &= filterBuilder.Regex(
                x => x.Name,
                new BsonRegularExpression(request.Filter, "i")
            );
        }

        var query = _collection.Find(filter);
        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();
        var dtos = items.Select(x => new LookupDto(x.Id, x.Name)).ToList();
        return new PaginatedResponse<LookupDto>
        {
            Items = dtos,
            TotalCount = (int)total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }
}
