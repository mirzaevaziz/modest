using FluentValidation;

namespace Modest.Core.Features.References.Product;

public class ProductUpdateDtoValidator : ProductBaseValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        SetupSharedRules(x => x.Name, x => x.Manufacturer, x => x.Country, x => x.PieceCountInUnit);
    }
}
