using System.Linq.Expressions;
using FluentValidation;
using static Modest.Core.Features.References.Product.ProductConstants;

namespace Modest.Core.Features.References.Product;

public abstract class ProductBaseValidator<T> : AbstractValidator<T>
{
    protected void SetupSharedRules(
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, string>> manufacturerSelector,
        Expression<Func<T, string>> countrySelector,
        Expression<Func<T, int>> pieceCountInUnitSelector
    )
    {
        RuleFor(nameSelector).NotEmpty().MinimumLength(NameMinLength).MaximumLength(NameMaxLength);

        RuleFor(manufacturerSelector)
            .NotEmpty()
            .MinimumLength(ManufacturerMinLength)
            .MaximumLength(ManufacturerMaxLength);

        RuleFor(countrySelector)
            .NotEmpty()
            .MinimumLength(CountryMinLength)
            .MaximumLength(CountryMaxLength);

        RuleFor(pieceCountInUnitSelector)
            .GreaterThanOrEqualTo(PieceCountInUnitMin)
            .LessThanOrEqualTo(PieceCountInUnitMax)
            .WithMessage(
                $"PieceCountInUnit must be between {PieceCountInUnitMin} and {PieceCountInUnitMax}."
            );
    }
}
