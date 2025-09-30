using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Modest.Core.Features.References.Product;

public class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public const string CollectionName = "Products";

    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToCollection(CollectionName);
        // Configure the primary key
        builder.HasKey(p => p.Id);

        // Property configurations
        builder.Property(p => p.Name).IsRequired().HasMaxLength(ProductConstants.NameMaxLength);
        builder
            .Property(p => p.Country)
            .IsRequired()
            .HasMaxLength(ProductConstants.CountryMaxLength);
        builder
            .Property(p => p.Manufacturer)
            .IsRequired()
            .HasMaxLength(ProductConstants.ManufacturerMaxLength);

        // Unique index on FullName
        builder.HasIndex(p => p.FullName).IsUnique();
        builder.Property(p => p.FullName).IsRequired();

        // Auditing (optional: map common timestamps)
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired().IsConcurrencyToken();
        builder.Property(p => p.DeletedAt).IsRequired(false);
        builder.Property(p => p.IsDeleted).IsRequired();
    }
}
