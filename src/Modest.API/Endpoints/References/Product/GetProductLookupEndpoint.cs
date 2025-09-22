using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetProductLookupEndpoint(IProductService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResponse<LookupDto>>
{
    public override void Configure()
    {
        Get("/products/lookup");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get product lookup paginated";
            s.Description =
                "Returns a paginated lookup list of products for dropdowns or quick search.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await service.GetProductLookupDtosAsync(req);
        await Send.OkAsync(result, ct);
    }
}
