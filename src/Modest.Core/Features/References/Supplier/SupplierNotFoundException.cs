using Modest.Core.Common.Exceptions;

namespace Modest.Core.Features.References.Supplier;

public class SupplierNotFoundException : ItemNotFoundException
{
    public SupplierNotFoundException()
        : base("Supplier was not found.") { }

    public SupplierNotFoundException(Guid id)
        : base($"Supplier with ID '{id}' was not found.") { }
}
