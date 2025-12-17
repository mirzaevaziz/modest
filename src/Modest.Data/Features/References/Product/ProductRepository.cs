using Modest.Core.Common.Models;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Product;
using Modest.Core.Helpers;
using Modest.Data.Common;
using MongoDB.Driver;

namespace Modest.Data.Features.References.Product;

public class ProductRepository
    : BaseRepository<ProductEntity, ProductDto, ProductFilter>,
        IProductRepository
{
    private const string CollectionName = "product";

    public ProductRepository(IMongoDatabase database, ICurrentUserProvider currentUserProvider)
        : base(database, CollectionName, currentUserProvider) { }

    protected override void EnsureIndexes()
    {
        // Create unique index for Code
        CreateUniqueIndex("code", x => x.Code);

        // Create unique index for FullName
        CreateUniqueIndex("fullname", x => x.FullName);
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
            filter = ApplyShowDeletedFilter(filter, request.Filter.ShowDeleted);
            filter = ApplySearchTextFilter(filter, request.Filter.SearchText, x => x.FullName);

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
            filter = filterBuilder.Eq(x => x.IsDeleted, false);
        }

        var query = Collection.Find(filter);
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

    public Task<PaginatedResponse<ProductLookupDto>> GetProductLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return GetLookupAsync(
            request,
            x => x.FullName,
            entity => new ProductLookupDto(entity.Id, entity.FullName, entity.PieceCountInUnit)
        );
    }

    public Task<PaginatedResponse<string>> GetManufacturerLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return GetDistinctFieldLookupAsync(x => x.Manufacturer, request);
    }

    public Task<PaginatedResponse<string>> GetCountryLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return GetDistinctFieldLookupAsync(x => x.Country, request);
    }

    public Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        return GetByIdAsync(id, entity => entity.ToProductDto());
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto, string code)
    {
        using var session = await Collection.Database.Client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var currentUser = CurrentUserProvider.GetCurrentUsername();
            var entity = new ProductEntity
            {
                Code = code,
                Name = productCreateDto.Name,
                Manufacturer = productCreateDto.Manufacturer,
                Country = productCreateDto.Country,
                PieceCountInUnit = productCreateDto.PieceCountInUnit,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser,
                IsDeleted = false,
            };

            var duplicate = await Collection
                .Find(x => x.FullName == entity.FullName)
                .FirstOrDefaultAsync();
            if (duplicate != null)
            {
                if (!duplicate.IsDeleted)
                {
                    throw new FluentValidation.ValidationException(
                        "Product with the same FullName already exists."
                    );
                }

                await RestoreDeletedEntityAsync(
                    session,
                    duplicate,
                    e =>
                    {
                        e.Name = productCreateDto.Name;
                        e.Manufacturer = productCreateDto.Manufacturer;
                        e.Country = productCreateDto.Country;
                        e.PieceCountInUnit = productCreateDto.PieceCountInUnit;
                    }
                );
                await session.CommitTransactionAsync();
                return duplicate.ToProductDto();
            }

            await Collection.InsertOneAsync(session, entity);
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
        var currentUser = CurrentUserProvider.GetCurrentUsername();
        var entity =
            await Collection
                .Find(x => x.Id == productUpdateDto.Id && !x.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new ProductNotFoundException(productUpdateDto.Id);
        entity.Name = productUpdateDto.Name;
        entity.Manufacturer = productUpdateDto.Manufacturer;
        entity.Country = productUpdateDto.Country;
        entity.PieceCountInUnit = productUpdateDto.PieceCountInUnit;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = currentUser;
        var duplicate = await Collection
            .Find(x => x.FullName == entity.FullName && x.Id != entity.Id)
            .FirstOrDefaultAsync();
        if (duplicate is not null)
        {
            if (!duplicate.IsDeleted)
            {
                throw new FluentValidation.ValidationException(
                    "Product with the same FullName already exists."
                );
            }
            // Rename deleted duplicate to avoid conflict
            duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
            await Collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
        }

        await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity.ToProductDto();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var currentUser = CurrentUserProvider.GetCurrentUsername();
        var entity = await Collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = currentUser;
        var result = await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }
}
