using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.Data.Features.References.Product;

public static class ProductMappers
{
    public static ProductDto ToDto(this ProductEntity entity)
    {
        return new ProductDto(
            entity.Id,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt,
            entity.CreatedBy,
            entity.UpdatedBy,
            entity.DeletedBy,
            entity.Code,
            entity.FullName,
            entity.Name,
            entity.Manufacturer,
            entity.Country,
            entity.PieceCountInUnit
        );
    }

    public static LookupDto ToLookupDto(this ProductEntity entity)
    {
        return new LookupDto(entity.Id, entity.FullName);
    }

    public static ProductLookupDto ToDto(this ProductLookupEntity entity)
    {
        return new ProductLookupDto(entity.Id, entity.Name, entity.PieceCountInUnit);
    }
}
