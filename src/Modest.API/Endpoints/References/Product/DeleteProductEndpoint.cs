using FastEndpoints;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class DeleteProductEndpoint(IProductService service)
    : Endpoint<DeleteProductEndpoint.DeleteProductRequest, bool>
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

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        var result = await service.DeleteProductAsync(req.Id);
        await Send.OkAsync(result, ct);
    }

    public class DeleteProductRequest
    {
        public Guid Id { get; set; }
    }
}
