using Modest.API.Endpoints.Common;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class GetSupplierLookupEndpoint(ISupplierService service)
    : BaseLookupEndpoint<ISupplierService, SupplierLookupDto>(service)
{
    protected override string ResourcePath => "/references/suppliers";
    protected override string ResourceName => "supplier";

    protected override Task<PaginatedResponse<SupplierLookupDto>> GetLookupAsync(
        ISupplierService service,
        PaginatedRequest<string> request
    )
    {
        return service.GetSupplierLookupDtosAsync(request);
    }
}
