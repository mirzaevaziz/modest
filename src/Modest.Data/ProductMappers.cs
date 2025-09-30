using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.Data;

public static class ProductMappers
{
    public static ProductDto ToProductDto(this ProductEntity entity)
    {
        return new ProductDto(
            entity.Id,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt,
            entity.FullName,
            entity.Name,
            entity.Manufacturer,
            entity.Country
        );
    }

    public static LookupDto ToLookupDto(this ProductEntity entity)
    {
        return new LookupDto(entity.Id, entity.FullName);
    }
}
