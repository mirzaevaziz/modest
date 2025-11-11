using Modest.Core.Features.References.Supplier;

namespace Modest.Data.Features.References.Supplier;

public static class SupplierMappers
{
    public static SupplierDto ToDto(this SupplierEntity entity)
    {
        return new SupplierDto(
            Id: entity.Id,
            IsDeleted: entity.IsDeleted,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt,
            DeletedAt: entity.DeletedAt,
            CreatedBy: entity.CreatedBy,
            UpdatedBy: entity.UpdatedBy,
            DeletedBy: entity.DeletedBy,
            Code: entity.Code,
            Name: entity.Name,
            ContactPerson: entity.ContactPerson,
            Phone: entity.Phone,
            Email: entity.Email,
            Address: entity.Address
        );
    }

    public static SupplierLookupDto ToDto(this SupplierLookupEntity entity)
    {
        return new SupplierLookupDto(Id: entity.Id, Name: entity.Name, Code: entity.Code);
    }
}
