using FluentValidation;

namespace Modest.Core.Helpers;

public static class ValidationHelper
{
    public static void ValidateAndThrow<T>(T instance, IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IValidator<T>)) is not IValidator<T> validator)
        {
            throw new InvalidOperationException(
                $"No validator registered for type {typeof(T).Name}"
            );
        }

        var result = validator.Validate(instance);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
