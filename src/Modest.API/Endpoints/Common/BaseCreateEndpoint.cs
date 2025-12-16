using FastEndpoints;

namespace Modest.API.Endpoints.Common;

public abstract class BaseCreateEndpoint<TService, TCreateDto, TDto>(TService service)
    : Endpoint<TCreateDto, TDto>
    where TCreateDto : notnull
{
    protected abstract string ResourcePath { get; }
    protected abstract string ResourceName { get; }

    public override void Configure()
    {
        Post(ResourcePath);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Create a new {ResourceName}";
            s.Description = $"Creates a new {ResourceName} with the provided details.";
        });
    }

    public override async Task HandleAsync(TCreateDto req, CancellationToken ct)
    {
        var result = await CreateAsync(service, req);
        await Send.OkAsync(result, ct);
    }

    protected abstract Task<TDto> CreateAsync(TService service, TCreateDto dto);
}
