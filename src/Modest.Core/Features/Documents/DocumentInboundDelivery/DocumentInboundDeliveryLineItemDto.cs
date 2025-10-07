using Modest.Core.Features.References.Product;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryLineItemDto(
    Guid Id,
    ProductLookupDto Product,
    decimal Quantity,
    decimal UnitPrice,
    decimal PiecePrice,
    decimal TotalPrice,
    string? Comment
);
