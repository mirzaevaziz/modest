namespace Modest.Core.Common.Models;

public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
