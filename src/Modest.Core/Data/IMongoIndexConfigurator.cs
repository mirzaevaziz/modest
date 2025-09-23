using MongoDB.Driver;

namespace Modest.Core.Data;

public interface IMongoIndexConfigurator
{
    void CreateIndexes(IMongoDatabase database);
}
