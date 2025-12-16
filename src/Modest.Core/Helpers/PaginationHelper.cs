using Modest.Core.Common.Models;

namespace Modest.Core.Helpers;

public static class PaginationHelper
{
    /// <summary>
    /// Builds a PaginatedResponse from a collection of items and total count.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    /// <param name="items">The items for the current page.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated response containing the items and metadata.</returns>
    public static PaginatedResponse<T> BuildResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize
    )
    {
        return new PaginatedResponse<T>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
    }
}
