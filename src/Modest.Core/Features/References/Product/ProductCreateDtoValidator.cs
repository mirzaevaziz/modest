using FluentValidation;
using static Modest.Core.Features.References.Product.ProductConstants;

namespace Modest.Core.Features.References.Product;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(NameMinLength).MaximumLength(NameMaxLength);

        RuleFor(x => x.Manufacturer)
            .NotEmpty()
            .MinimumLength(ManufacturerMinLength)
            .MaximumLength(ManufacturerMaxLength);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MinimumLength(CountryMinLength)
            .MaximumLength(CountryMaxLength);
    }
}
