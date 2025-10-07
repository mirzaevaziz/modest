namespace Modest.Core.Features.References.Product;

public record ProductDto(
    Guid Id,
    bool IsDeleted,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    string? CreatedBy,
    string? UpdatedBy,
    string? DeletedBy,
    string Code,
    string FullName,
    string Name,
    string Manufacturer,
    string Country,
    int PieceCountInUnit
);
