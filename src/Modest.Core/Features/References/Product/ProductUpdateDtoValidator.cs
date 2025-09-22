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
        RuleFor(x => x.Manufacturer).NotEmpty().MaximumLength(ManufacturerMaxLength);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(CountryMaxLength);
    }
}
