using System.Linq.Expressions;
using FluentValidation;
using static Modest.Core.Features.References.Supplier.SupplierConstants;

namespace Modest.Core.Features.References.Supplier;

public abstract class SupplierBaseValidator<T> : AbstractValidator<T>
{
    protected void SetupSharedRules(
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, string?>> contactPersonSelector,
        Expression<Func<T, string?>> phoneSelector,
        Expression<Func<T, string?>> emailSelector,
        Expression<Func<T, string?>> addressSelector
    )
    {
        RuleFor(nameSelector)
            .NotEmpty()
            .WithMessage("Supplier Name is required.")
            .MinimumLength(NameMinLength)
            .WithMessage($"Supplier Name must be at least {NameMinLength} characters.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Supplier Name must not exceed {NameMaxLength} characters.");

        RuleFor(contactPersonSelector)
            .MaximumLength(ContactPersonMaxLength)
            .When(x => !string.IsNullOrEmpty(contactPersonSelector.Compile()(x)))
            .WithMessage($"Contact Person must not exceed {ContactPersonMaxLength} characters.");

        RuleFor(phoneSelector)
            .MaximumLength(PhoneMaxLength)
            .When(x => !string.IsNullOrEmpty(phoneSelector.Compile()(x)))
            .WithMessage($"Phone must not exceed {PhoneMaxLength} characters.");

        RuleFor(emailSelector)
            .MaximumLength(EmailMaxLength)
            .When(x => !string.IsNullOrEmpty(emailSelector.Compile()(x)))
            .WithMessage($"Email must not exceed {EmailMaxLength} characters.")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(emailSelector.Compile()(x)))
            .WithMessage("Email must be a valid email address.");

        RuleFor(addressSelector)
            .MaximumLength(AddressMaxLength)
            .When(x => !string.IsNullOrEmpty(addressSelector.Compile()(x)))
            .WithMessage($"Address must not exceed {AddressMaxLength} characters.");
    }
}
