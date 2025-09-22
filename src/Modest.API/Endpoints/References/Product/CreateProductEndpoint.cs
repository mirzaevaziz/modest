using FastEndpoints;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class CreateProductEndpoint(IProductService service) : Endpoint<ProductCreateDto, ProductDto>
{
    public override void Configure()
    {
        Post("/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new product";
            s.Description = "Creates a new product with the provided details.";
        });
    }

    public override async Task HandleAsync(ProductCreateDto req, CancellationToken ct)
    {
        var product = await service.CreateProductAsync(req);
        await Send.OkAsync(product, ct);
    }
}
