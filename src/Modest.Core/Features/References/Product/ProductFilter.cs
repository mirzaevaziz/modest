namespace Modest.Core.Features.References.Product;

public record ProductFilter(
    string? SearchText,
    string? Manufacturer,
    string? Country,
    bool? ShowDeleted
);
