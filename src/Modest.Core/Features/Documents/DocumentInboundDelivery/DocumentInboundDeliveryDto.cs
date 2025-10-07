namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryDto(
    Guid Id,
    bool IsDeleted,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    string Number,
    DateTimeOffset Date,
    string SupplierName,
    string SupplierCode,
    bool IsSigned,
    string? Comment,
    int LineItemCount,
    decimal TotalAmount,
    List<DocumentInboundDeliveryLineItemDto> LineItems
);
