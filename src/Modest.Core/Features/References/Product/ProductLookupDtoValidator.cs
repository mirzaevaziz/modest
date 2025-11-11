using FluentValidation;
using static Modest.Core.Features.References.Product.ProductConstants;

namespace Modest.Core.Features.References.Product;

public class ProductLookupDtoValidator : AbstractValidator<ProductLookupDto>
{
    public ProductLookupDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product Name is required.")
            .MinimumLength(NameMinLength)
            .WithMessage($"Product Name must be at least {NameMinLength} characters.");

        RuleFor(x => x.PieceCountInUnit)
            .GreaterThanOrEqualTo(PieceCountInUnitMin)
            .LessThanOrEqualTo(PieceCountInUnitMax)
            .WithMessage(
                $"PieceCountInUnit must be between {PieceCountInUnitMin} and {PieceCountInUnitMax}."
            );
    }
}
