using Modest.Core.Features.Documents.DocumentInboundDelivery;

namespace Modest.Data.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryStatusHistoryEntity
{
    public Guid Id { get; set; }
    public DocumentInboundDeliveryStatus Status { get; set; }
    public required string ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Comment { get; set; }
}
