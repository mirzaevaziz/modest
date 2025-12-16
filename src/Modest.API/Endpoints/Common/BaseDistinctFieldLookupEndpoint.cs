using FastEndpoints;
using Modest.Core.Common.Models;

namespace Modest.API.Endpoints.Common;

public abstract class BaseDistinctFieldLookupEndpoint<TService>(TService service)
    : Endpoint<PaginatedRequest<string>, PaginatedResponse<string>>
{
    protected abstract string ResourcePath { get; }
    protected abstract string FieldName { get; }

    public override void Configure()
    {
        Get($"{ResourcePath}/{FieldName}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Get {FieldName} lookup paginated";
            s.Description =
                $"Returns a paginated, distinct list of {FieldName} values, optionally filtered by a search string.";
        });
    }

    public override async Task HandleAsync(PaginatedRequest<string> req, CancellationToken ct)
    {
        var result = await GetDistinctFieldLookupAsync(service, req);
        await Send.OkAsync(result, ct);
    }

    protected abstract Task<PaginatedResponse<string>> GetDistinctFieldLookupAsync(
        TService service,
        PaginatedRequest<string> request
    );
}
