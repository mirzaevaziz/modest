// ...existing code...
using FluentValidation;
using static Modest.Core.Features.References.Product.ProductConstants;

namespace Modest.Core.Features.References.Product;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(NameMinLength).MaximumLength(NameMaxLength);

        RuleFor(x => x.Manufacturer)
            .NotEmpty()
            .MinimumLength(ManufacturerMinLength)
            .MaximumLength(ManufacturerMaxLength);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MinimumLength(CountryMinLength)
            .MaximumLength(CountryMaxLength);

        RuleFor(x => x.PieceCountInUnit)
            .GreaterThanOrEqualTo(PieceCountInUnitMin)
            .LessThanOrEqualTo(PieceCountInUnitMax)
            .WithMessage(
                $"PieceCountInUnit must be between {PieceCountInUnitMin} and {PieceCountInUnitMax}."
            );
    }
}
