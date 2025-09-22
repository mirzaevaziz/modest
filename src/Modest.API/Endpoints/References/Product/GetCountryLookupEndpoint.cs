using FastEndpoints;
using Modest.Core.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetCountryLookupEndpoint(IProductService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResult<string>>
{
    public override void Configure()
    {
        Get("/products/countries");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated, distinct country names for products.";
            s.Description =
                "Returns a paginated list of unique country names from products, optionally filtered by a search string.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await service.GetCountryLookupDtosAsync(req);
        await Send.OkAsync(result, ct);
    }
}
