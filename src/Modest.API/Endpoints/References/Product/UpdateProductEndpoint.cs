using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class UpdateProductEndpoint(IProductService service)
    : BaseUpdateEndpoint<IProductService, ProductUpdateDto, ProductDto>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string ResourceName => "product";

    protected override Task<ProductDto> UpdateAsync(IProductService service, ProductUpdateDto dto)
    {
        return service.UpdateProductAsync(dto);
    }
}
