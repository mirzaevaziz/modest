using FluentValidation;
using static Modest.Core.Features.References.Supplier.SupplierConstants;

namespace Modest.Core.Features.References.Supplier;

public class SupplierUpdateDtoValidator : AbstractValidator<SupplierUpdateDto>
{
    public SupplierUpdateDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Supplier Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Supplier Name is required.")
            .MinimumLength(NameMinLength)
            .WithMessage($"Supplier Name must be at least {NameMinLength} characters.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Supplier Name must not exceed {NameMaxLength} characters.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(ContactPersonMaxLength)
            .When(x => !string.IsNullOrEmpty(x.ContactPerson))
            .WithMessage($"Contact Person must not exceed {ContactPersonMaxLength} characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(PhoneMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage($"Phone must not exceed {PhoneMaxLength} characters.");

        RuleFor(x => x.Email)
            .MaximumLength(EmailMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage($"Email must not exceed {EmailMaxLength} characters.")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Address)
            .MaximumLength(AddressMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage($"Address must not exceed {AddressMaxLength} characters.");
    }
}
