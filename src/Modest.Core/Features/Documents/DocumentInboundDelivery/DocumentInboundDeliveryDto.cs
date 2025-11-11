using Modest.Core.Features.References.Supplier;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryDto(
    Guid Id,
    bool IsDeleted,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    string? CreatedBy,
    string? UpdatedBy,
    string? DeletedBy,
    string Number,
    DateTimeOffset Date,
    SupplierLookupDto Supplier,
    string? SupplierDocumentNumber,
    DateTimeOffset? SupplierDocumentDate,
    DocumentInboundDeliveryStatus CurrentStatus,
    string? Comment,
    int LineItemCount,
    decimal TotalCost,
    List<DocumentInboundDeliveryLineItemDto> LineItemList,
    List<DocumentInboundDeliveryStatusHistoryDto> StatusHistoryList
);
