using Microsoft.EntityFrameworkCore;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;

namespace Modest.Core.Data;

public class ModestDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; init; }

    public ModestDbContext(DbContextOptions options)
        : base(options)
    {
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditableEntityRules();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditableEntityRules();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditableEntityRules()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.IsDeleted = false; // Ensure IsDeleted is false for new entities
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified; // Switch to Modified to implement soft delete
                    entry.Entity.DeletedAt = now;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }
    }
}
