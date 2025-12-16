using Modest.Data.Common;

namespace Modest.Data.Features.References.Supplier;

public class SupplierEntity : AuditableEntity, ICodeEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
