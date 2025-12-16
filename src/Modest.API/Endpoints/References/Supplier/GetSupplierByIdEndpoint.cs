using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class GetSupplierByIdEndpoint(ISupplierService service) : Endpoint<IdRequest, SupplierDto>
{
    public override void Configure()
    {
        Get("/references/suppliers/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get supplier by id";
            s.Description = "Returns a supplier by its unique identifier.";
        });
    }

    public override async Task HandleAsync(IdRequest req, CancellationToken ct)
    {
        var supplier = await service.GetSupplierByIdAsync(req.Id);
        if (supplier is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(supplier, ct);
    }
}
