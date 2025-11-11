using FluentValidation;
using Modest.Core.Features.References.Supplier;
using static Modest.Core.Features.Documents.DocumentInboundDelivery.DocumentInboundDeliveryConstants;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryCreateDtoValidator
    : AbstractValidator<DocumentInboundDeliveryCreateDto>
{
    public DocumentInboundDeliveryCreateDtoValidator()
    {
        RuleFor(x => x.Supplier)
            .NotNull()
            .WithMessage("Supplier is required.")
            .SetValidator(new SupplierLookupDtoValidator());

        RuleFor(x => x.SupplierDocumentNumber)
            .MaximumLength(SupplierDocumentNumberMaxLength)
            .When(x => !string.IsNullOrEmpty(x.SupplierDocumentNumber))
            .WithMessage(
                $"Supplier Document Number must not exceed {SupplierDocumentNumberMaxLength} characters."
            );

        RuleFor(x => x.SupplierDocumentDate)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(1))
            .When(x => x.SupplierDocumentDate.HasValue)
            .WithMessage("Supplier Document Date cannot be in the future.");

        RuleFor(x => x.Comment)
            .MaximumLength(CommentMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage($"Comment must not exceed {CommentMaxLength} characters.");

        RuleForEach(x => x.LineItemList)
            .SetValidator(new DocumentInboundDeliveryLineItemCreateDtoValidator())
            .When(x => x.LineItemList != null && x.LineItemList.Count > 0);
    }
}
