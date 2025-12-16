using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class UpdateSupplierEndpoint(ISupplierService service)
    : BaseUpdateEndpoint<ISupplierService, SupplierUpdateDto, SupplierDto>(service)
{
    protected override string ResourcePath => "/references/suppliers";
    protected override string ResourceName => "supplier";

    protected override Task<SupplierDto> UpdateAsync(
        ISupplierService service,
        SupplierUpdateDto dto
    )
    {
        return service.UpdateSupplierAsync(dto);
    }
}
