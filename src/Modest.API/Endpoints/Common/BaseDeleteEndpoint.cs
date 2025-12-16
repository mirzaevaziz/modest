using FastEndpoints;
using Modest.Core.Common.Models;

namespace Modest.API.Endpoints.Common;

public abstract class BaseDeleteEndpoint<TService>(TService service) : Endpoint<IdRequest, bool>
{
    protected abstract string ResourcePath { get; }
    protected abstract string ResourceName { get; }

    public override void Configure()
    {
        Delete($"{ResourcePath}/{{id:guid}}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Delete a {ResourceName}";
            s.Description = $"Deletes a {ResourceName} by its unique identifier.";
        });
    }

    public override async Task HandleAsync(IdRequest req, CancellationToken ct)
    {
        var result = await DeleteAsync(service, req.Id);
        await Send.OkAsync(result, ct);
    }

    protected abstract Task<bool> DeleteAsync(TService service, Guid id);
}
