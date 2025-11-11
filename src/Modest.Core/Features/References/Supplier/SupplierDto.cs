namespace Modest.Core.Features.References.Supplier;

public record SupplierDto(
    Guid Id,
    bool IsDeleted,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt,
    string? CreatedBy,
    string? UpdatedBy,
    string? DeletedBy,
    string Code,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address
);
