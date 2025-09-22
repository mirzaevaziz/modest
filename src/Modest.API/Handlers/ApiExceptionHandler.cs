using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Modest.Core.Common.Exceptions;

namespace Modest.API.Handlers;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, object[], Exception?> _validationLog =
        LoggerMessage.Define<object[]>(
            LogLevel.Warning,
            new EventId(1, nameof(ValidationException)),
            "Validation failed: {Errors}"
        );

    private static readonly Action<ILogger, string, Exception?> _notFoundLog =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(ItemNotFoundException)),
            "Not found: {Message}"
        );

    private static readonly Action<ILogger, Exception> _unhandledLog = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(3, "UnhandledException"),
        "Unhandled exception occurred"
    );

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        httpContext.Response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException validationException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = validationException
                    .Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                    .ToArray();
                var validationResult = JsonSerializer.Serialize(
                    new { ErrorMessage = "Validation failed.", Errors = errors }
                );
                _validationLog(logger, errors, validationException);
                await httpContext.Response.WriteAsync(validationResult, cancellationToken);
                return true;

            case ItemNotFoundException notFoundException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                var notFoundResult = JsonSerializer.Serialize(
                    new { ErrorMessage = notFoundException.Message }
                );
                _notFoundLog(logger, notFoundException.Message, notFoundException);
                await httpContext.Response.WriteAsync(notFoundResult, cancellationToken);
                return true;

            default:
                _unhandledLog(logger, exception);
                return false;
        }
    }
}
