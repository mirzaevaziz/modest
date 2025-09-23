using Modest.Core.Data;
using MongoDB.Driver;

namespace Modest.Core.Features.References.Product;

public class ProductEntityIndexConfigurator : IMongoIndexConfigurator
{
    public void CreateIndexes(IMongoDatabase database)
    {
        var collection = database.GetCollection<ProductEntity>("Products");
        var indexKeys = Builders<ProductEntity>.IndexKeys.Ascending(p => p.FullName);
        var indexOptions = new CreateIndexOptions { Unique = true, Name = "Unique_FullName" };
        var indexModel = new CreateIndexModel<ProductEntity>(indexKeys, indexOptions);
        collection.Indexes.CreateOne(indexModel);
    }
}
