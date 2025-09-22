using FastEndpoints;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetProductByIdEndpoint(IProductService service)
    : Endpoint<GetProductByIdEndpoint.GetProductByIdRequest, ProductDto>
{
    public override void Configure()
    {
        Get("/references/products/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get product by id";
            s.Description = "Returns a product by its unique identifier.";
        });
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var product = await service.GetProductByIdAsync(req.Id);
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(product, ct);
    }

    public class GetProductByIdRequest
    {
        public Guid Id { get; set; }
    }
}
