using Modest.Core.Features.Documents.DocumentInboundDelivery;
using Modest.Data.Features.References.Supplier;

namespace Modest.Data.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryEntity : AuditableEntity
{
    public required string Number { get; set; }
    public DateTimeOffset Date { get; set; }
    public required SupplierLookupEntity Supplier { get; set; }
    public string? SupplierDocumentNumber { get; set; }
    public DateTimeOffset? SupplierDocumentDate { get; set; }
    public DocumentInboundDeliveryStatus CurrentStatus { get; set; }
    public string? Comment { get; set; }
    public int LineItemCount { get; set; }
    public decimal TotalCost { get; set; }
    public List<DocumentInboundDeliveryLineItemEntity> LineItemList { get; set; } = new();
    public List<DocumentInboundDeliveryStatusHistoryEntity> StatusHistoryList { get; set; } = new();
}
