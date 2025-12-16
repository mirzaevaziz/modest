using FluentValidation;

namespace Modest.Core.Features.References.Supplier;

public class SupplierUpdateDtoValidator : SupplierBaseValidator<SupplierUpdateDto>
{
    public SupplierUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        SetupSharedRules(
            x => x.Name,
            x => x.ContactPerson,
            x => x.Phone,
            x => x.Email,
            x => x.Address
        );
    }
}
