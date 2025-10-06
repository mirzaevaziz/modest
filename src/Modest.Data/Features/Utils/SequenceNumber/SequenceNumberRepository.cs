using Modest.Core.Features.Utils.SequenceNumber;
using MongoDB.Driver;

namespace Modest.Data.Features.Utils.SequenceNumber;

public class SequenceNumberRepository : ISequenceNumberRepository
{
    private readonly IMongoCollection<SequenceNumberEntity> _collection;

    public SequenceNumberRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SequenceNumberEntity>("sequence_numbers");
    }

    public async Task<long> GetNextSequenceAsync(string prefix)
    {
        var filter = Builders<SequenceNumberEntity>.Filter.Eq(x => x.Prefix, prefix);
        var update = Builders<SequenceNumberEntity>.Update.Inc(x => x.Value, 1);
        var options = new FindOneAndUpdateOptions<SequenceNumberEntity>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
        };

        var result = await _collection.FindOneAndUpdateAsync(filter, update, options);
        return result.Value;
    }
}
