using Modest.Data.Common;

namespace Modest.Data.Features.References.Product;

public class ProductEntity : AuditableEntity, ICodeEntity
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
    public required int PieceCountInUnit { get; set; }
}
