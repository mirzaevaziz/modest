using FastEndpoints;
using Modest.Core.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetManufacturerLookupEndpoint(IProductService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResult<string>>
{
    public override void Configure()
    {
        Get("/products/manufacturers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get manufacturer lookup paginated";
            s.Description =
                "Returns a paginated, distinct list of manufacturer names for products.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await service.GetManufacturerLookupDtosAsync(req);
        await Send.OkAsync(result, ct);
    }
}
