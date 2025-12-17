using System.Linq.Expressions;
using FluentValidation;
using Modest.Core.Common.Models;
using Modest.Core.Features.Auth;
using Modest.Core.Helpers;
using MongoDB.Driver;

namespace Modest.Data.Common;

public abstract class BaseRepository<TEntity, TDto, TFilter>
    where TEntity : AuditableEntity, ICodeEntity
    where TFilter : class
{
    protected IMongoCollection<TEntity> Collection { get; }
    protected ICurrentUserProvider CurrentUserProvider { get; }

    protected BaseRepository(
        IMongoDatabase database,
        string collectionName,
        ICurrentUserProvider currentUserProvider
    )
    {
        Collection = database.GetCollection<TEntity>(collectionName);
        CurrentUserProvider = currentUserProvider;
        EnsureIndexes();
    }

    protected abstract void EnsureIndexes();

    protected void CreateUniqueIndex(string fieldName, Expression<Func<TEntity, object>> field)
    {
        var indexKeys = Builders<TEntity>.IndexKeys.Ascending(field);
        var indexModel = new CreateIndexModel<TEntity>(
            indexKeys,
            new CreateIndexOptions
            {
                Unique = true,
                Name = $"idx_{fieldName}",
                Background = true,
            }
        );

        try
        {
            Collection.Indexes.CreateOne(indexModel);
        }
        catch (MongoCommandException ex)
            when (ex.CodeName is "IndexOptionsConflict" or "IndexKeySpecsConflict")
        {
            // Index already exists, ignore
        }
    }

    protected async Task<PaginatedResponse<TLookupDto>> GetLookupAsync<TLookupDto>(
        PaginatedRequest<string> request,
        Expression<Func<TEntity, string>> searchField,
        Func<TEntity, TLookupDto> toLookupDto
    )
    {
        var filterBuilder = Builders<TEntity>.Filter;
        var filter = filterBuilder.Eq(x => x.IsDeleted, false);

        if (!string.IsNullOrEmpty(request.Filter))
        {
            filter &= MongoFilterHelper.BuildWordSearchFilter(request.Filter, searchField);
        }

        var query = Collection.Find(filter);
        var total = await query.CountDocumentsAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync();

        var dtos = items.Select(toLookupDto).ToList();
        return PaginationHelper.BuildResponse(
            dtos,
            (int)total,
            request.PageNumber,
            request.PageSize
        );
    }

    protected async Task<PaginatedResponse<string>> GetDistinctFieldLookupAsync(
        Expression<Func<TEntity, string>> fieldSelector,
        PaginatedRequest<string> request
    )
    {
        var distinctValues = await Collection.DistinctAsync(
            fieldSelector,
            Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false)
        );
        var list = await distinctValues.ToListAsync();

        if (!string.IsNullOrEmpty(request.Filter))
        {
            list = list.Where(value =>
                    value.Contains(request.Filter, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
        }

        var total = list.Count;
        var paged = list.Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return PaginationHelper.BuildResponse(paged, total, request.PageNumber, request.PageSize);
    }

    protected async Task<TDto?> GetByIdAsync(Guid id, Func<TEntity, TDto> toDto)
    {
        var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return entity == null ? default : toDto(entity);
    }

    protected FilterDefinition<TEntity> ApplyShowDeletedFilter(
        FilterDefinition<TEntity> filter,
        bool? showDeleted
    )
    {
        var filterBuilder = Builders<TEntity>.Filter;
        if (showDeleted.HasValue)
        {
            filter &= filterBuilder.Eq(x => x.IsDeleted, showDeleted.Value);
        }
        else
        {
            filter &= filterBuilder.Eq(x => x.IsDeleted, false);
        }

        return filter;
    }

    protected FilterDefinition<TEntity> ApplySearchTextFilter(
        FilterDefinition<TEntity> filter,
        string? searchText,
        Expression<Func<TEntity, string>> searchField
    )
    {
        if (!string.IsNullOrEmpty(searchText))
        {
            filter &= MongoFilterHelper.BuildWordSearchFilter(searchText, searchField);
        }

        return filter;
    }

    protected async Task RestoreDeletedEntityAsync(
        IClientSessionHandle session,
        TEntity entity,
        Action<TEntity> updateFields
    )
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        updateFields(entity);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = CurrentUserProvider.GetCurrentUsername();
        await Collection.ReplaceOneAsync(session, x => x.Id == entity.Id, entity);
    }
}
