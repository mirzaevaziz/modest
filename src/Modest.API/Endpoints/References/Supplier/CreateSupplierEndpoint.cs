using FastEndpoints;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class CreateSupplierEndpoint(ISupplierService service)
    : Endpoint<SupplierCreateDto, SupplierDto>
{
    public override void Configure()
    {
        Post("/references/suppliers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new supplier";
            s.Description = "Creates a new supplier with the provided details.";
        });
    }

    public override async Task HandleAsync(SupplierCreateDto req, CancellationToken ct)
    {
        var supplier = await service.CreateSupplierAsync(req);
        await Send.OkAsync(supplier, ct);
    }
}
