using FastEndpoints;
using Modest.Core.Common;
using Modest.Core.Features.References.Product;

namespace Modest.API.Endpoints.References.Product;

public class GetAllProductsEndpoint(IProductService service)
    : Endpoint<GetAllProductsEndpoint.GetAllProductsRequest, List<ProductDto>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all products (paginated, filter, sort)";
            s.Description = "Returns a paginated list of products. Supports filtering and sorting.";
        });
    }

    public override async Task HandleAsync(GetAllProductsRequest req, CancellationToken ct)
    {
        var result = await service.GetAllProductsAsync(
            new PaginatedRequest<ProductFilter>
            {
                Filter = req.Filter,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize,
            },
            req.SortFields
        );
        await Send.OkAsync(result.Items.ToList(), ct);
    }

    public class GetAllProductsRequest
    {
        public ProductFilter? Filter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<SortField>? SortFields { get; set; }
    }
}
