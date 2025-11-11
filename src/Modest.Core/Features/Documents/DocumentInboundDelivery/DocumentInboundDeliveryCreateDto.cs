using Modest.Core.Features.References.Supplier;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryCreateDto(
    SupplierLookupDto Supplier,
    string? SupplierDocumentNumber,
    DateTimeOffset? SupplierDocumentDate,
    string? Comment,
    List<DocumentInboundDeliveryLineItemCreateDto> LineItemList
);
