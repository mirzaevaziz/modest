namespace Modest.Data.Features.References.Product;

/// <summary>
/// Lightweight entity for product lookups - embedded in other documents
/// </summary>
public class ProductLookupEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required int PieceCountInUnit { get; set; }
}
