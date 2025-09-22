using System.Linq.Expressions;
using Modest.Core.Common;

namespace Modest.Core.Helpers;

public static class SortingHelper
{
    /// <summary>
    /// Applies sorting dynamically based on a single sort field.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity or DTO.</typeparam>
    /// <param name="query">The query to apply sorting on.</param>
    /// <param name="sortField">The single sorting rule.</param>
    /// <returns>The query with sorting applied.</returns>
    public static IQueryable<TEntity> ApplySorting<TEntity>(
        IQueryable<TEntity> query,
        SortField sortField
    )
    {
        return ApplyMultipleSorting(query, new List<SortField> { sortField });
    }

    /// <summary>
    /// Applies sorting dynamically based on multiple sorting rules.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity or DTO.</typeparam>
    /// <param name="query">The query to apply sorting on.</param>
    /// <param name="sortFields">The list of sorting rules.</param>
    /// <returns>The query with sorting applied.</returns>
    public static IQueryable<TEntity> ApplyMultipleSorting<TEntity>(
        IQueryable<TEntity> query,
        IEnumerable<SortField> sortFields
    )
    {
        if (sortFields == null || !sortFields.Any())
        {
            return query; // No sorting if no fields are specified
        }

        IOrderedQueryable<TEntity>? orderedQuery = null;

        for (var i = 0; i < sortFields.Count(); i++)
        {
            var sortField = sortFields.ElementAt(i);
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.PropertyOrField(parameter, sortField.FieldName);
            var keySelector = Expression.Lambda(property, parameter);

            var methodName =
                i == 0
                    ? (sortField.Ascending ? "OrderBy" : "OrderByDescending")
                    : (sortField.Ascending ? "ThenBy" : "ThenByDescending");

            var method = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), property.Type);

            orderedQuery = (IOrderedQueryable<TEntity>?)
                method.Invoke(null, [orderedQuery ?? query, keySelector]);
        }

        return orderedQuery ?? query;
    }
}
