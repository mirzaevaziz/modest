namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public record DocumentInboundDeliveryStatusHistoryDto(
    Guid Id,
    DocumentInboundDeliveryStatus Status,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string? Comment
);
