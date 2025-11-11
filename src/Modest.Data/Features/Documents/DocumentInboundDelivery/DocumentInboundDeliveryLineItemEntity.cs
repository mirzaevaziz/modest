using Modest.Data.Features.References.Product;

namespace Modest.Data.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryLineItemEntity
{
    public Guid Id { get; set; }
    public required ProductLookupEntity Product { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal Price { get; set; }
    public required decimal VAT { get; set; }
    public string? Comment { get; set; }
}
