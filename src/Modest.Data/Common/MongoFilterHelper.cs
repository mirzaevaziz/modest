using System.Linq.Expressions;
using Modest.Core.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Modest.Data.Common;

public static class MongoFilterHelper
{
    /// <summary>
    /// Builds a word search filter that splits the search text into words and creates
    /// a filter for each word using regex. All words must match (AND logic).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter</typeparam>
    /// <param name="searchText">The search text to split into words</param>
    /// <param name="fieldSelector">Expression to select the field to search in</param>
    /// <returns>A filter definition that matches all words in the specified field</returns>
    public static FilterDefinition<TEntity> BuildWordSearchFilter<TEntity>(
        string searchText,
        Expression<Func<TEntity, string>> fieldSelector
    )
    {
        var filterBuilder = Builders<TEntity>.Filter;

        // Split the search text into words and create a filter for each word
        var words = searchText.Split(
            Constants.WordSeparators,
            StringSplitOptions.RemoveEmptyEntries
        );

        if (words.Length == 0)
        {
            return filterBuilder.Empty;
        }

        var wordFilters = new List<FilterDefinition<TEntity>>();
        foreach (var word in words)
        {
            var regex = new BsonRegularExpression(word, "i");
            FieldDefinition<TEntity, string> field = new ExpressionFieldDefinition<TEntity, string>(
                fieldSelector
            );
            wordFilters.Add(filterBuilder.Regex(field, regex));
        }

        // Combine all word filters with AND logic (all words must match)
        return filterBuilder.And(wordFilters);
    }
}
