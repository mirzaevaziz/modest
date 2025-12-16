using FastEndpoints;
using Modest.Core.Common.Models;

namespace Modest.API.Endpoints.Common;

public abstract class BaseGetByIdEndpoint<TService, TDto>(TService service)
    : Endpoint<IdRequest, TDto>
    where TDto : class
{
    protected abstract string ResourcePath { get; }
    protected abstract string ResourceName { get; }

    public override void Configure()
    {
        Get($"{ResourcePath}/{{id:guid}}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Get {ResourceName} by id";
            s.Description = $"Returns a {ResourceName} by its unique identifier.";
        });
    }

    public override async Task HandleAsync(IdRequest req, CancellationToken ct)
    {
        var result = await GetByIdAsync(service, req.Id);
        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }

    protected abstract Task<TDto?> GetByIdAsync(TService service, Guid id);
}
