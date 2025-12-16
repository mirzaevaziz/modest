using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class DeleteProductEndpoint(IProductService service)
    : BaseDeleteEndpoint<IProductService>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string ResourceName => "product";

    protected override Task<bool> DeleteAsync(IProductService service, Guid id)
    {
        return service.DeleteProductAsync(id);
    }
}
