using MongoDB.Bson.Serialization.Attributes;

namespace Modest.Data.Features.References.Product;

[BsonIgnoreExtraElements]
public class ProductEntity : AuditableEntity
{
    public string FullName
    {
        get { return $"{Name}({Manufacturer}/{Country})"; }
        set { }
    }

    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Manufacturer { get; set; }
    public required string Country { get; set; }
}
