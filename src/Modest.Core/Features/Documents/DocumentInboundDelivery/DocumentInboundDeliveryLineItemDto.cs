using Modest.Core.Features.References.Product;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryLineItemDto(
    Guid Id,
    ProductLookupDto Product,
    int UnitQuantity,
    int PieceQuantity,
    decimal UnitPrice,
    decimal PiecePrice,
    decimal TotalPrice,
    decimal VAT,
    string? Comment
);
