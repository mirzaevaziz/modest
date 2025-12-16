using Modest.API.Endpoints.Common;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetManufacturerLookupEndpoint(IProductService service)
    : BaseDistinctFieldLookupEndpoint<IProductService>(service)
{
    protected override string ResourcePath => "/references/products";
    protected override string FieldName => "manufacturers";

    protected override Task<PaginatedResponse<string>> GetDistinctFieldLookupAsync(
        IProductService service,
        PaginatedRequest<string> request
    )
    {
        return service.GetManufacturerLookupDtosAsync(request);
    }
}
