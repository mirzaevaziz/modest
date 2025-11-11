using FluentValidation;
using Modest.Core.Features.References.Product;
using static Modest.Core.Features.Documents.DocumentInboundDelivery.DocumentInboundDeliveryConstants;

namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public class DocumentInboundDeliveryLineItemUpdateDtoValidator
    : AbstractValidator<DocumentInboundDeliveryLineItemUpdateDto>
{
    public DocumentInboundDeliveryLineItemUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Line item Id is required.");

        RuleFor(x => x.Product)
            .NotNull()
            .WithMessage("Product is required.")
            .SetValidator(new ProductLookupDtoValidator());

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(MinQuantity)
            .WithMessage($"Quantity must be at least {MinQuantity}.")
            .LessThanOrEqualTo(MaxQuantity)
            .WithMessage($"Quantity must not exceed {MaxQuantity}.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(MinPrice)
            .WithMessage($"Price must be at least {MinPrice}.")
            .LessThanOrEqualTo(MaxPrice)
            .WithMessage($"Price must not exceed {MaxPrice}.");

        RuleFor(x => x.VAT)
            .GreaterThanOrEqualTo(MinVAT)
            .WithMessage($"VAT must be at least {MinVAT}%.")
            .LessThanOrEqualTo(MaxVAT)
            .WithMessage($"VAT must not exceed {MaxVAT}%.");

        RuleFor(x => x.Comment)
            .MaximumLength(CommentMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage($"Comment must not exceed {CommentMaxLength} characters.");
    }
}
