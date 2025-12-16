using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class CreateProductEndpoint(IProductService service)
    : BaseCreateEndpoint<IProductService, ProductCreateDto, ProductDto>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string ResourceName => "product";

    protected override Task<ProductDto> CreateAsync(IProductService service, ProductCreateDto dto)
    {
        return service.CreateProductAsync(dto);
    }
}
