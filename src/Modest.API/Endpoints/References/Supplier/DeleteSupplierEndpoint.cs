using FastEndpoints;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class DeleteSupplierEndpoint(ISupplierService service)
    : Endpoint<DeleteSupplierEndpoint.DeleteSupplierRequest, bool>
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

    public override async Task HandleAsync(DeleteSupplierRequest req, CancellationToken ct)
    {
        var result = await service.DeleteSupplierAsync(req.Id);
        await Send.OkAsync(result, ct);
    }

    public class DeleteSupplierRequest
    {
        public Guid Id { get; set; }
    }
}
