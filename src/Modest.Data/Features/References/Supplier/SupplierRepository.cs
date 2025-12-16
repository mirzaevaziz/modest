using FluentValidation;
using Modest.Core.Common.Models;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Supplier;
using Modest.Core.Helpers;
using Modest.Data.Common;
using MongoDB.Driver;

namespace Modest.Data.Features.References.Supplier;

public class SupplierRepository : ISupplierRepository
{
    private const string CollectionName = "supplier";

    private readonly IMongoCollection<SupplierEntity> _collection;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SupplierRepository(IMongoDatabase database, ICurrentUserProvider currentUserProvider)
    {
        _collection = database.GetCollection<SupplierEntity>(CollectionName);
        _currentUserProvider = currentUserProvider;

        // Ensure index for Code (unique)
        var codeIndexKeys = Builders<SupplierEntity>.IndexKeys.Ascending(x => x.Code);
        var codeIndexModel = new CreateIndexModel<SupplierEntity>(
            codeIndexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = "idx_code",
                Background = true,
            }
        );

        // Ensure index for Name
        var nameIndexKeys = Builders<SupplierEntity>.IndexKeys.Ascending(x => x.Name);
        var nameIndexModel = new CreateIndexModel<SupplierEntity>(
            nameIndexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = "idx_name",
                Background = true,
            }
        );

        try
        {
            _collection.Indexes.CreateOne(codeIndexModel);
            _collection.Indexes.CreateOne(nameIndexModel);
        }
        catch (MongoCommandException ex)
            when (ex.CodeName is "IndexOptionsConflict" or "IndexKeySpecsConflict")
        {
            // Index already exists, ignore
        }
    }

    public async Task<PaginatedResponse<SupplierDto>> GetAllSuppliersAsync(
        PaginatedRequest<SupplierFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        var filterBuilder = Builders<SupplierEntity>.Filter;
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
                filter &= MongoFilterHelper.BuildWordSearchFilter<SupplierEntity>(
                    request.Filter.SearchText,
                    x => x.Name
                );
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
            var sortDef = Builders<SupplierEntity>.Sort.Combine(
                sortFields.Select(sf =>
                    sf.Ascending
                        ? Builders<SupplierEntity>.Sort.Ascending(sf.FieldName)
                        : Builders<SupplierEntity>.Sort.Descending(sf.FieldName)
                )
            );
            query = query.Sort(sortDef);
        }

        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();
        var dtos = items.Select(x => x.ToDto()).ToList();
        return PaginationHelper.BuildResponse(
            dtos,
            (int)total,
            request.PageNumber,
            request.PageSize
        );
    }

    public async Task<PaginatedResponse<SupplierLookupDto>> GetSupplierLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        var filterBuilder = Builders<SupplierEntity>.Filter;
        var filter = filterBuilder.Eq(x => x.IsDeleted, false);
        if (!string.IsNullOrEmpty(request.Filter))
        {
            filter &= MongoFilterHelper.BuildWordSearchFilter<SupplierEntity>(
                request.Filter,
                x => x.Name
            );
        }

        var query = _collection.Find(filter);
        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();
        var dtos = items.Select(x => new SupplierLookupDto(x.Id, x.Name, x.Code)).ToList();
        return PaginationHelper.BuildResponse(
            dtos,
            (int)total,
            request.PageNumber,
            request.PageSize
        );
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid id)
    {
        var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return entity?.ToDto();
    }

    public async Task<SupplierDto> CreateSupplierAsync(
        SupplierCreateDto supplierCreateDto,
        string code
    )
    {
        using var session = await _collection.Database.Client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var currentUser = _currentUserProvider.GetCurrentUsername();
            var entity = new SupplierEntity
            {
                Code = code,
                Name = supplierCreateDto.Name,
                ContactPerson = supplierCreateDto.ContactPerson,
                Phone = supplierCreateDto.Phone,
                Email = supplierCreateDto.Email,
                Address = supplierCreateDto.Address,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser,
                IsDeleted = false,
            };

            // Check for duplicate Name
            var duplicate = await _collection
                .Find(x => x.Name == entity.Name)
                .FirstOrDefaultAsync();
            if (duplicate != null)
            {
                if (!duplicate.IsDeleted)
                {
                    throw new ValidationException(
                        $"Supplier with Name '{entity.Name}' already exists."
                    );
                }
                // Restore
                duplicate.IsDeleted = false;
                duplicate.DeletedAt = null;
                duplicate.DeletedBy = null;
                duplicate.Name = supplierCreateDto.Name;
                duplicate.ContactPerson = supplierCreateDto.ContactPerson;
                duplicate.Phone = supplierCreateDto.Phone;
                duplicate.Email = supplierCreateDto.Email;
                duplicate.Address = supplierCreateDto.Address;
                duplicate.UpdatedAt = DateTimeOffset.UtcNow;
                duplicate.UpdatedBy = currentUser;
                await _collection.ReplaceOneAsync(session, x => x.Id == duplicate.Id, duplicate);
                await session.CommitTransactionAsync();
                return duplicate.ToDto();
            }

            await _collection.InsertOneAsync(session, entity);
            await session.CommitTransactionAsync();
            return entity.ToDto();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<SupplierDto> UpdateSupplierAsync(SupplierUpdateDto supplierUpdateDto)
    {
        var currentUser = _currentUserProvider.GetCurrentUsername();
        var entity =
            await _collection
                .Find(x => x.Id == supplierUpdateDto.Id && !x.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new SupplierNotFoundException(supplierUpdateDto.Id);

        entity.Name = supplierUpdateDto.Name;
        entity.ContactPerson = supplierUpdateDto.ContactPerson;
        entity.Phone = supplierUpdateDto.Phone;
        entity.Email = supplierUpdateDto.Email;
        entity.Address = supplierUpdateDto.Address;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = currentUser;
        var duplicate = await _collection
            .Find(x => x.Name == entity.Name && x.Id != entity.Id)
            .FirstOrDefaultAsync();
        if (duplicate is not null)
        {
            if (!duplicate.IsDeleted)
            {
                throw new ValidationException($"Supplier with the same Name already exists.");
            }
            // Change the name of the deleted duplicate to avoid conflict
            duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
            await _collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
        }

        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity.ToDto();
    }

    public async Task<bool> DeleteSupplierAsync(Guid id)
    {
        var currentUser = _currentUserProvider.GetCurrentUsername();
        var entity = await _collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = currentUser;
        var result = await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }
}
