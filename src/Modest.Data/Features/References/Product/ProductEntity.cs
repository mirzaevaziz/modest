namespace Modest.Data.Features.References.Product;

public class ProductEntity : AuditableEntity
{
    public string FullName
    {
        get { return $"{Name}({Manufacturer}/{Country})"; }
        set { }
    }

    public required string Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? Country { get; set; }
}
