using FluentValidation;
using static Modest.Core.Features.References.Supplier.SupplierConstants;

namespace Modest.Core.Features.References.Supplier;

public class SupplierLookupDtoValidator : AbstractValidator<SupplierLookupDto>
{
    public SupplierLookupDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Supplier Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Supplier Name is required.")
            .MinimumLength(NameMinLength)
            .WithMessage($"Supplier Name must be at least {NameMinLength} characters.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Supplier Name must not exceed {NameMaxLength} characters.");
    }
}
