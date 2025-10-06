using MongoDB.Bson.Serialization.Attributes;

namespace Modest.Data.Features.Utils.SequenceNumber;

[BsonIgnoreExtraElements]
public class SequenceNumberEntity
{
    [BsonId]
    public string Prefix { get; set; } = default!;

    public long Value { get; set; }
}
