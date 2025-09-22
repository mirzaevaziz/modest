using Modest.Core.Common;

namespace Modest.Core.Features.References.Product;

public class ProductNotFoundException : ItemNotFoundException
{
    public ProductNotFoundException(Guid id)
        : base($"Product with Id '{id}' was not found.") { }
}
