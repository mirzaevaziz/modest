using FluentValidation;

namespace Modest.Core.Helpers;

public static class ValidationHelper
{
    public static void ValidateAndThrow<T>(T instance, IValidator<T> validator)
    {
        var result = validator.Validate(instance);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
