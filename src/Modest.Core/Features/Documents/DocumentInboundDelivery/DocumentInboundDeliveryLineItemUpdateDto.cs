using Modest.Core.Features.References.Product;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryLineItemUpdateDto(
    Guid Id,
    ProductLookupDto Product,
    decimal Quantity,
    decimal Price,
    decimal VAT,
    string? Comment
);
