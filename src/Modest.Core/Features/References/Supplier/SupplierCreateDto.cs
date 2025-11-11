namespace Modest.Core.Features.References.Supplier;

public record SupplierCreateDto(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address
);
