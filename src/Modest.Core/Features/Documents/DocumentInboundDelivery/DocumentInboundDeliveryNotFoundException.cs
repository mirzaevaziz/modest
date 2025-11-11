namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryNotFoundException : Exception
{
    public DocumentInboundDeliveryNotFoundException(Guid id)
        : base($"Document with ID '{id}' was not found.") { }
}
