using FluentValidation;
using Modest.Core.Common.Models;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Product;
using Modest.Core.Helpers;
using Modest.Data.Common;
using MongoDB.Driver;

namespace Modest.Data.Features.References.Product;

public class ProductRepository : IProductRepository
{
    private const string CollectionName = "product";

    private readonly IMongoCollection<ProductEntity> _collection;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ProductRepository(IMongoDatabase database, ICurrentUserProvider currentUserProvider)
    {
        _collection = database.GetCollection<ProductEntity>(CollectionName);
        _currentUserProvider = currentUserProvider;

        // Ensure index for Code (unique)
        var codeIndexKeys = Builders<ProductEntity>.IndexKeys.Ascending(x => x.Code);
        var codeIndexModel = new CreateIndexModel<ProductEntity>(
            codeIndexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = "idx_code",
                Background = true, // Non-blocking index creation
            }
        );

        // Ensure index for FullName (compound index on Name, Manufacturer, Country)
        // Note: CreateOne with background:true is idempotent and won't recreate if exists
        var fullNameIndexKeys = Builders<ProductEntity>.IndexKeys.Ascending(x => x.FullName);
        var fullNameIndexModel = new CreateIndexModel<ProductEntity>(
            fullNameIndexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = "idx_fullname",
                Background = true, // Non-blocking index creation
            }
        );

        try
        {
            _collection.Indexes.CreateOne(codeIndexModel);
            _collection.Indexes.CreateOne(fullNameIndexModel);
        }
        catch (MongoCommandException ex)
            when (ex.CodeName is "IndexOptionsConflict" or "IndexKeySpecsConflict")
        {
            // Index already exists with different options or same name, ignore
        }
    }

    public async Task<PaginatedResponse<ProductDto>> GetAllProductsAsync(
        PaginatedRequest<ProductFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        var filterBuilder = Builders<ProductEntity>.Filter;
        var filter = filterBuilder.Empty;

        if (request.Filter != null)
        {
            // Handle ShowDeleted filter
            if (request.Filter.ShowDeleted.HasValue)
            {
                filter &= filterBuilder.Eq(x => x.IsDeleted, request.Filter.ShowDeleted.Value);
            }
            else
            {
                // Default: only show non-deleted items
                filter &= filterBuilder.Eq(x => x.IsDeleted, false);
            }

            if (!string.IsNullOrEmpty(request.Filter.SearchText))
            {
                filter &= MongoFilterHelper.BuildWordSearchFilter<ProductEntity>(
                    request.Filter.SearchText,
                    x => x.FullName
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
        else
        {
            // Default: only show non-deleted items when no filter is provided
            filter = filterBuilder.Eq(x => x.IsDeleted, false);
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
        return PaginationHelper.BuildResponse(
            dtos,
            (int)total,
            request.PageNumber,
            request.PageSize
        );
    }

    public async Task<PaginatedResponse<ProductLookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var filterBuilder = Builders<ProductEntity>.Filter;
        var filter = filterBuilder.Eq(x => x.IsDeleted, false);
        if (!string.IsNullOrEmpty(request.Filter))
        {
            filter &= MongoFilterHelper.BuildWordSearchFilter<ProductEntity>(
                request.Filter,
                x => x.FullName
            );
        }

        var query = _collection.Find(filter);
        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();
        var dtos = items
            .Select(x => new ProductLookupDto(x.Id, x.FullName, x.PieceCountInUnit))
            .ToList();
        return PaginationHelper.BuildResponse(
            dtos,
            (int)total,
            request.PageNumber,
            request.PageSize
        );
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
        return PaginationHelper.BuildResponse(paged, total, request.PageNumber, request.PageSize);
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
        return PaginationHelper.BuildResponse(paged, total, request.PageNumber, request.PageSize);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return entity?.ToProductDto();
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto, string code)
    {
        using var session = await _collection.Database.Client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var currentUser = _currentUserProvider.GetCurrentUsername();
            var entity = new ProductEntity
            {
                Code = code,
                Name = productCreateDto.Name,
                Manufacturer = productCreateDto.Manufacturer,
                Country = productCreateDto.Country,
                PieceCountInUnit = productCreateDto.PieceCountInUnit,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser,
                IsDeleted = false,
            };

            var duplicate = await _collection
                .Find(x => x.FullName == entity.FullName)
                .FirstOrDefaultAsync();
            if (duplicate != null)
            {
                if (!duplicate.IsDeleted)
                {
                    throw new ValidationException(
                        $"Product with the same FullName already exists."
                    );
                }
                // Undo delete (restore)
                duplicate.IsDeleted = false;
                duplicate.DeletedAt = null;
                duplicate.DeletedBy = null;
                duplicate.Name = productCreateDto.Name;
                duplicate.Manufacturer = productCreateDto.Manufacturer;
                duplicate.Country = productCreateDto.Country;
                duplicate.PieceCountInUnit = productCreateDto.PieceCountInUnit;
                duplicate.UpdatedAt = DateTime.UtcNow;
                duplicate.UpdatedBy = currentUser;
                await _collection.ReplaceOneAsync(session, x => x.Id == duplicate.Id, duplicate);
                await session.CommitTransactionAsync();
                return duplicate.ToProductDto();
            }

            await _collection.InsertOneAsync(session, entity);
            await session.CommitTransactionAsync();
            return entity.ToProductDto();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(ProductUpdateDto productUpdateDto)
    {
        var currentUser = _currentUserProvider.GetCurrentUsername();
        var entity =
            await _collection
                .Find(x => x.Id == productUpdateDto.Id && !x.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new ProductNotFoundException(productUpdateDto.Id);
        entity.Name = productUpdateDto.Name;
        entity.Manufacturer = productUpdateDto.Manufacturer;
        entity.Country = productUpdateDto.Country;
        entity.PieceCountInUnit = productUpdateDto.PieceCountInUnit;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser;
        var duplicate = await _collection
            .Find(x => x.FullName == entity.FullName && x.Id != entity.Id)
            .FirstOrDefaultAsync();
        if (duplicate is not null)
        {
            if (!duplicate.IsDeleted)
            {
                throw new ValidationException($"Product with the same FullName already exists.");
            }
            // Change the name of the deleted duplicate to avoid conflict
            duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
            await _collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
        }

        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity.ToProductDto();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var currentUser = _currentUserProvider.GetCurrentUsername();
        var entity = await _collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = currentUser;
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }
}
