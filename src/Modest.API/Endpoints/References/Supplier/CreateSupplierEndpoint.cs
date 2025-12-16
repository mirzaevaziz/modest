using Modest.API.Endpoints.Common;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class CreateSupplierEndpoint(ISupplierService service)
    : BaseCreateEndpoint<ISupplierService, SupplierCreateDto, SupplierDto>(service)
{
    protected override string ResourcePath => "/references/suppliers";
    protected override string ResourceName => "supplier";

    protected override Task<SupplierDto> CreateAsync(
        ISupplierService service,
        SupplierCreateDto dto
    )
    {
        return service.CreateSupplierAsync(dto);
    }
}
