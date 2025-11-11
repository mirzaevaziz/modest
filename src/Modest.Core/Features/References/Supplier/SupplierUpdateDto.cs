namespace Modest.Core.Features.References.Supplier;

public record SupplierUpdateDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address
);
