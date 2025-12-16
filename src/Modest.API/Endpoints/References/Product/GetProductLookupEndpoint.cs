using Modest.API.Endpoints.Common;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetProductLookupEndpoint(IProductService service)
    : BaseLookupEndpoint<IProductService, ProductLookupDto>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string ResourceName => "product";

    protected override Task<PaginatedResponse<ProductLookupDto>> GetLookupAsync(
        IProductService service,
        PaginatedRequest<string> request
    )
    {
        return service.GetProductLookupDtosAsync(request);
    }
}
