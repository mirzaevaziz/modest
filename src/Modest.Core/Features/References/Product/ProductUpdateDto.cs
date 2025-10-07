namespace Modest.Core.Features.References.Product;

public record ProductUpdateDto(
    Guid Id,
    string Name,
    string Manufacturer,
    string Country,
    int PieceCountInUnit
);
