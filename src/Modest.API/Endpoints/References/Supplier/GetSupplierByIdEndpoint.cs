using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class GetSupplierByIdEndpoint(ISupplierService service)
    : BaseGetByIdEndpoint<ISupplierService, SupplierDto>(service)
{
    protected override string ResourcePath => "/references/suppliers";
    protected override string ResourceName => "supplier";

    protected override Task<SupplierDto?> GetByIdAsync(ISupplierService service, Guid id)
    {
        return service.GetSupplierByIdAsync(id);
    }
}
