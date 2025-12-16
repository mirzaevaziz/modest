using Modest.Core.Common.Models;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Supplier;
using Modest.Core.Helpers;
using Modest.Data.Common;
using MongoDB.Driver;

namespace Modest.Data.Features.References.Supplier;

public class SupplierRepository
    : BaseRepository<
        SupplierEntity,
        SupplierDto,
        SupplierCreateDto,
        SupplierUpdateDto,
        SupplierFilter
    >,
        ISupplierRepository
{
    private const string CollectionName = "supplier";

    public SupplierRepository(IMongoDatabase database, ICurrentUserProvider currentUserProvider)
        : base(database, CollectionName, currentUserProvider) { }

    protected override void EnsureIndexes()
    {
        // Create unique index for Code
        CreateUniqueIndex("code", x => x.Code);

        // Create unique index for Name
        CreateUniqueIndex("name", x => x.Name);
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
            filter = ApplyShowDeletedFilter(filter, request.Filter.ShowDeleted);
            filter = ApplySearchTextFilter(filter, request.Filter.SearchText, x => x.Name);
        }
        else
        {
            filter = filterBuilder.Eq(x => x.IsDeleted, false);
        }

        var query = Collection.Find(filter);
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

    public Task<PaginatedResponse<SupplierLookupDto>> GetSupplierLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return GetLookupAsync(
            request,
            x => x.Name,
            entity => new SupplierLookupDto(entity.Id, entity.Name, entity.Code)
        );
    }

    public Task<SupplierDto?> GetSupplierByIdAsync(Guid id)
    {
        return GetByIdAsync(id, entity => entity.ToDto());
    }

    public async Task<SupplierDto> CreateSupplierAsync(
        SupplierCreateDto supplierCreateDto,
        string code
    )
    {
        using var session = await Collection.Database.Client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var currentUser = CurrentUserProvider.GetCurrentUsername();
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

            var duplicate = await Collection.Find(x => x.Name == entity.Name).FirstOrDefaultAsync();
            if (duplicate != null)
            {
                if (!duplicate.IsDeleted)
                {
                    throw new FluentValidation.ValidationException(
                        "Supplier with the same name already exists."
                    );
                }
                // Restore deleted duplicate
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
                await Collection.ReplaceOneAsync(session, x => x.Id == duplicate.Id, duplicate);
                await session.CommitTransactionAsync();
                return duplicate.ToDto();
            }

            await Collection.InsertOneAsync(session, entity);
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
        var currentUser = CurrentUserProvider.GetCurrentUsername();
        var entity =
            await Collection
                .Find(x => x.Id == supplierUpdateDto.Id && !x.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new SupplierNotFoundException(supplierUpdateDto.Id);
        entity.Name = supplierUpdateDto.Name;
        entity.ContactPerson = supplierUpdateDto.ContactPerson;
        entity.Phone = supplierUpdateDto.Phone;
        entity.Email = supplierUpdateDto.Email;
        entity.Address = supplierUpdateDto.Address;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = currentUser;
        var duplicate = await Collection
            .Find(x => x.Name == entity.Name && x.Id != entity.Id)
            .FirstOrDefaultAsync();
        if (duplicate is not null)
        {
            if (!duplicate.IsDeleted)
            {
                throw new FluentValidation.ValidationException(
                    "Supplier with the same name already exists."
                );
            }
            // Rename deleted duplicate to avoid conflict
            duplicate.Name += $" - Changed {DateTimeOffset.UtcNow}";
            await Collection.ReplaceOneAsync(x => x.Id == duplicate.Id, duplicate);
        }

        await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity.ToDto();
    }

    public async Task<bool> DeleteSupplierAsync(Guid id)
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
