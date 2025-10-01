namespace Modest.Core.Features.References.Product;

public record ProductDto(
    Guid Id,
    bool IsDeleted,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    string FullName,
    string Name,
    string Manufacturer,
    string Country
);
