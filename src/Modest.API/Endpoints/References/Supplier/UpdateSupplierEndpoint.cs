using FastEndpoints;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class UpdateSupplierEndpoint(ISupplierService service)
    : Endpoint<SupplierUpdateDto, SupplierDto>
{
    public override void Configure()
    {
        Put("/references/suppliers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a supplier";
            s.Description = "Updates an existing supplier with the provided details.";
        });
    }

    public override async Task HandleAsync(SupplierUpdateDto req, CancellationToken ct)
    {
        var supplier = await service.UpdateSupplierAsync(req);
        await Send.OkAsync(supplier, ct);
    }
}
