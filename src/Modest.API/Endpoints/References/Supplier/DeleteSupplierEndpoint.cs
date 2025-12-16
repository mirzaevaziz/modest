using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class DeleteSupplierEndpoint(ISupplierService service) : Endpoint<IdRequest, bool>
{
    public override void Configure()
    {
        Delete("/references/suppliers/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a supplier";
            s.Description = "Deletes a supplier by its unique identifier.";
        });
    }

    public override async Task HandleAsync(IdRequest req, CancellationToken ct)
    {
        var result = await service.DeleteSupplierAsync(req.Id);
        await Send.OkAsync(result, ct);
    }
}
