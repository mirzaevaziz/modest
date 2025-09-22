using Modest.Core.Common.Exceptions;

namespace Modest.Core.Features.References.Product;

public class ProductNotFoundException : ItemNotFoundException
{
    public ProductNotFoundException(Guid id)
        : base($"Product with Id '{id}' was not found.") { }
}
