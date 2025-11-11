using Microsoft.Extensions.Logging;

namespace Modest.Core.Features.References.Supplier;

internal static class SupplierServiceLog
{
    public static readonly Action<ILogger, string, string?, string?, Exception?> CreatingSupplier =
        LoggerMessage.Define<string, string?, string?>(
            LogLevel.Information,
            new EventId(1, nameof(CreatingSupplier)),
            "Creating supplier: Name={Name}, ContactPerson={ContactPerson}, Phone={Phone}"
        );

    public static readonly Action<ILogger, Guid, string, Exception?> SupplierCreated =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2, nameof(SupplierCreated)),
            "Supplier created successfully: Id={Id}, Name={Name}"
        );

    public static readonly Action<
        ILogger,
        Guid,
        string,
        string?,
        string?,
        Exception?
    > UpdatingSupplier = LoggerMessage.Define<Guid, string, string?, string?>(
        LogLevel.Information,
        new EventId(3, nameof(UpdatingSupplier)),
        "Updating supplier: Id={Id}, Name={Name}, ContactPerson={ContactPerson}, Phone={Phone}"
    );

    public static readonly Action<ILogger, Guid, string, Exception?> SupplierUpdated =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(4, nameof(SupplierUpdated)),
            "Supplier updated successfully: Id={Id}, Name={Name}"
        );

    public static readonly Action<ILogger, Guid, Exception?> DeletingSupplier =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(5, nameof(DeletingSupplier)),
            "Deleting supplier: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, Exception?> SupplierDeleted =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(6, nameof(SupplierDeleted)),
            "Supplier deleted successfully: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, Exception?> SupplierDeleteFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(7, nameof(SupplierDeleteFailed)),
            "Supplier delete failed or supplier not found: Id={Id}"
        );

    public static readonly Action<ILogger, Guid, string, Exception?> UpdatedDuplicatedSupplier =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(8, nameof(UpdatedDuplicatedSupplier)),
            "Updated duplicated supplier with Id: {SupplierId}, Name: {Name}"
        );
}
