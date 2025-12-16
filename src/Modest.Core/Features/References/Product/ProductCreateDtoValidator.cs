namespace Modest.Core.Features.References.Product;

public class ProductCreateDtoValidator : ProductBaseValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        SetupSharedRules(x => x.Name, x => x.Manufacturer, x => x.Country, x => x.PieceCountInUnit);
    }
}
