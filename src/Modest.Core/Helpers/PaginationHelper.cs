using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Modest.Core.Common.Models;

namespace Modest.Core.Helpers;

public class PaginationHelper
{
    /// <summary>
    /// Retrieves a paginated list of projected DTOs with total count support.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the projected DTO.</typeparam>
    /// <param name="query">The base query to filter and paginate.</param>
    /// <param name="selector">A projection function to map the entity to a DTO.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing the DTOs and total count.</returns>
    public static async Task<PaginatedResponse<TDto>> PaginateAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TDto>> selector,
        int pageNumber,
        int pageSize,
        IEnumerable<SortFieldRequest> sortFields
    )
        where TEntity : class
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        // Calculate total count before pagination
        var totalCount = await query.CountAsync();

        query = SortingHelper.ApplyMultipleSorting(query, sortFields);

        // Apply pagination and projection
        var items = await query
            .Skip((pageNumber - 1) * pageSize) // Skip rows for previous pages
            .Take(pageSize) // Take only the rows for the current page
            .Select(selector) // Project to DTO
            .ToListAsync();

        // Return paginated result
        return new PaginatedResponse<TDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }
}
