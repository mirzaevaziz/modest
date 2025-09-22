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
            .MaximumLength(ManufacturerMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Manufacturer));

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(CountryMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}
