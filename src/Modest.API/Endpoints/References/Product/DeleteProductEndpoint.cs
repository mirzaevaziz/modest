using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class DeleteProductEndpoint(IProductService service) : Endpoint<IdRequest, bool>
{
    public override void Configure()
    {
        Delete("/references/products/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a product";
            s.Description = "Deletes a product by its unique identifier.";
        });
    }

    public override async Task HandleAsync(IdRequest req, CancellationToken ct)
    {
        var result = await service.DeleteProductAsync(req.Id);
        await Send.OkAsync(result, ct);
    }
}
