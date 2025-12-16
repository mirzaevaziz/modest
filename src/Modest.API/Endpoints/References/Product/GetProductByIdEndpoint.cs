using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetProductByIdEndpoint(IProductService service)
    : BaseGetByIdEndpoint<IProductService, ProductDto>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string ResourceName => "product";

    protected override Task<ProductDto?> GetByIdAsync(IProductService service, Guid id)
    {
        return service.GetProductByIdAsync(id);
    }
}
