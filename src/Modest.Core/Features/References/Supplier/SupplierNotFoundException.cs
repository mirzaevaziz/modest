namespace Modest.Core.Features.References.Supplier;

public class SupplierNotFoundException : Exception
{
    public SupplierNotFoundException(Guid id)
        : base($"Supplier with ID '{id}' was not found.") { }
}
