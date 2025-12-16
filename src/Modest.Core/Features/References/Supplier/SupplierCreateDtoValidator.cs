namespace Modest.Core.Features.References.Supplier;

public class SupplierCreateDtoValidator : SupplierBaseValidator<SupplierCreateDto>
{
    public SupplierCreateDtoValidator()
    {
        SetupSharedRules(
            x => x.Name,
            x => x.ContactPerson,
            x => x.Phone,
            x => x.Email,
            x => x.Address
        );
    }
}
