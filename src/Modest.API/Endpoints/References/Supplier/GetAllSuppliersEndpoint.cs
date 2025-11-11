using FastEndpoints;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;

namespace Modest.API.Endpoints.References.Supplier;

public class GetAllSuppliersEndpoint(ISupplierService service)
    : Endpoint<GetAllSuppliersEndpoint.GetAllSuppliersRequest, PaginatedResponse<SupplierDto>>
{
    public override void Configure()
    {
        Get("/references/suppliers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all suppliers (paginated, filter, sort)";
            s.Description =
                "Returns a paginated list of suppliers. Supports filtering and sorting.";
        });
    }

    public override async Task HandleAsync(GetAllSuppliersRequest req, CancellationToken ct)
    {
        var result = await service.GetAllSuppliersAsync(
            new PaginatedRequest<SupplierFilter>
            {
                Filter = req.Filter,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
            },
            req.SortFields
        );
        await Send.OkAsync(result, ct);
    }

    public class GetAllSuppliersRequest
    {
        public SupplierFilter? Filter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<SortFieldRequest>? SortFields { get; set; }
    }
}
