using Modest.Core.Features.References.Supplier;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryUpdateDto(
    Guid Id,
    string Number,
    DateTimeOffset Date,
    SupplierLookupDto Supplier,
    string? SupplierDocumentNumber,
    DateTimeOffset? SupplierDocumentDate,
    string? Comment
);
