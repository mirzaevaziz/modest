using System;
using Microsoft.Extensions.Logging;

namespace Modest.Core.Features.References.Product;

internal static class ProductServiceLog
{
    public static readonly Action<ILogger, string, string, string, Exception?> CreatingProduct =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(1, nameof(CreatingProduct)),
            "Creating product: Name={Name}, Manufacturer={Manufacturer}, Country={Country}"
        );

    public static readonly Action<ILogger, Guid, string, Exception?> ProductCreated =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2, nameof(ProductCreated)),
            "Product created successfully: Id={Id}, FullName={FullName}"
        );

    public static readonly Action<
        ILogger,
        Guid,
        string,
        string,
        string,
        Exception?
    > UpdatingProduct = LoggerMessage.Define<Guid, string, string, string>(
        LogLevel.Information,
        new EventId(3, nameof(UpdatingProduct)),
        "Updating product: Id={Id}, Name={Name}, Manufacturer={Manufacturer}, Country={Country}"
    );

    public static readonly Action<ILogger, Guid, string, Exception?> ProductUpdated =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(4, nameof(ProductUpdated)),
            "Product updated successfully: Id={Id}, FullName={FullName}"
        );

    public static readonly Action<ILogger, Guid, Exception?> DeletingProduct =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(5, nameof(DeletingProduct)),
            "Deleting product: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, Exception?> ProductDeleted =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(6, nameof(ProductDeleted)),
            "Product deleted successfully: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, Exception?> ProductDeleteFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(7, nameof(ProductDeleteFailed)),
            "Product delete failed or product not found: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, string, Exception?> UpdatedDuplicatedProduct =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(8, nameof(ProductDeleteFailed)),
            "Updated duplicated product with Id: {ProductId}, FullName: {FullName}"
        );
}
