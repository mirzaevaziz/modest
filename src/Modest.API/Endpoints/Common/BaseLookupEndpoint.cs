using FastEndpoints;
using Modest.Core.Common.Models;

namespace Modest.API.Endpoints.Common;

public abstract class BaseLookupEndpoint<TService, TLookupDto>(TService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResponse<TLookupDto>>
{
    protected abstract string ResourcePath { get; }
    protected abstract string ResourceName { get; }

    public override void Configure()
    {
        Get($"{ResourcePath}/lookup");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Get {ResourceName} lookup paginated";
            s.Description =
                $"Returns a paginated lookup list of {ResourceName}s for dropdowns or quick search.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await GetLookupAsync(service, req);
        await Send.OkAsync(result, ct);
    }

    protected abstract Task<PaginatedResponse<TLookupDto>> GetLookupAsync(
        TService service,
        PaginatedRequest<string> request
    );
}
