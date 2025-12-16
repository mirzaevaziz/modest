using FastEndpoints;

namespace Modest.API.Endpoints.Common;

public abstract class BaseUpdateEndpoint<TService, TUpdateDto, TDto>(TService service)
    : Endpoint<TUpdateDto, TDto>
    where TUpdateDto : notnull
{
    protected abstract string ResourcePath { get; }
    protected abstract string ResourceName { get; }

    public override void Configure()
    {
        Put(ResourcePath);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Update a {ResourceName}";
            s.Description = $"Updates an existing {ResourceName} with the provided details.";
        });
    }

    public override async Task HandleAsync(TUpdateDto req, CancellationToken ct)
    {
        var result = await UpdateAsync(service, req);
        await Send.OkAsync(result, ct);
    }

    protected abstract Task<TDto> UpdateAsync(TService service, TUpdateDto dto);
}
