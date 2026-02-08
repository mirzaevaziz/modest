using FluentValidation;
using Modest.Core.Common;
using Modest.Core.Features.References.Supplier;
using static Modest.Core.Features.Documents.DocumentInboundDelivery.DocumentInboundDeliveryConstants;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryUpdateDtoValidator
    : AbstractValidator<DocumentInboundDeliveryUpdateDto>
{
    public DocumentInboundDeliveryUpdateDtoValidator(ITimeProvider timeProvider)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Document Id is required.");

        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("Document Number is required.")
            .MinimumLength(NumberMinLength)
            .WithMessage($"Document Number must be at least {NumberMinLength} character.")
            .MaximumLength(NumberMaxLength)
            .WithMessage($"Document Number must not exceed {NumberMaxLength} characters.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Document Date is required.")
            .LessThanOrEqualTo(timeProvider.UtcNow.AddDays(1))
            .WithMessage("Document Date cannot be in the future.");

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
            .LessThanOrEqualTo(timeProvider.UtcNow.AddDays(1))
            .When(x => x.SupplierDocumentDate.HasValue)
            .WithMessage("Supplier Document Date cannot be in the future.");

        RuleFor(x => x.Comment)
            .MaximumLength(CommentMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage($"Comment must not exceed {CommentMaxLength} characters.");
    }
}
