using Modest.Core.Common.Models;

namespace Modest.Core.Features.References.Supplier;

public record SupplierLookupDto(Guid Id, string Name, string Code) : LookupDto(Id, Name);
