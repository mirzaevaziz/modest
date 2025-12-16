using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class DeleteSupplierEndpoint(ISupplierService service)
    : BaseDeleteEndpoint<ISupplierService>(service)
{
    protected override string ResourcePath => "/references/suppliers";
    protected override string ResourceName => "supplier";

    protected override Task<bool> DeleteAsync(ISupplierService service, Guid id)
    {
        return service.DeleteSupplierAsync(id);
    }
}
