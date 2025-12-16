using Modest.Core.Common.Exceptions;

namespace Modest.Core.Features.References.Product;

public class ProductNotFoundException : ItemNotFoundException
{
    public ProductNotFoundException()
        : base("Product was not found.") { }

    public ProductNotFoundException(Guid id)
        : base($"Product with Id '{id}' was not found.") { }
}
