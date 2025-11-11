namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public interface IDocumentInboundDeliveryRepository
{
    Task<DocumentInboundDeliveryDto> CreateDocumentInboundDeliveryAsync(
        DocumentInboundDeliveryCreateDto documentInboundDeliveryCreateDto,
        string number,
        DateTimeOffset documentDate
    );
    Task<DocumentInboundDeliveryDto> UpdateDocumentInboundDeliveryAsync(
        DocumentInboundDeliveryUpdateDto documentInboundDeliveryUpdateDto
    );
    Task<bool> DeleteDocumentInboundDeliveryAsync(Guid id);
    Task<DocumentInboundDeliveryDto?> GetDocumentInboundDeliveryByIdAsync(Guid id);
}
