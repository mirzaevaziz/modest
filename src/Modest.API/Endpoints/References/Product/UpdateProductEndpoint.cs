using FastEndpoints;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class UpdateProductEndpoint(IProductService service) : Endpoint<ProductUpdateDto, ProductDto>
{
    public override void Configure()
    {
        Put("/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a product";
            s.Description = "Updates an existing product with the provided details.";
        });
    }

    public override async Task HandleAsync(ProductUpdateDto req, CancellationToken ct)
    {
        var product = await service.UpdateProductAsync(req);
        await Send.OkAsync(product, ct);
    }
}
