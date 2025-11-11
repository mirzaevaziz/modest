using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class GetSupplierLookupEndpoint(ISupplierService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResponse<SupplierLookupDto>>
{
    public override void Configure()
    {
        Get("/references/suppliers/lookup");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get supplier lookup paginated";
            s.Description =
                "Returns a paginated lookup list of suppliers for dropdowns or quick search.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await service.GetSupplierLookupDtosAsync(req);
        await Send.OkAsync(result, ct);
    }
}
