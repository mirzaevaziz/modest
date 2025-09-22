using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modest.Core.Features.References.Product;

public class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        // Configure the primary key
        builder.HasKey(p => p.Id);

        // Property configurations

        builder.Property(p => p.Name).IsRequired().HasMaxLength(ProductConstants.NameMaxLength);
        builder.Property(p => p.Manufacturer).HasMaxLength(ProductConstants.ManufacturerMaxLength);
        builder.Property(p => p.Country).HasMaxLength(ProductConstants.CountryMaxLength);

        builder.Property(p => p.FullName).IsRequired();

        // Auditing (optional: map common timestamps)
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired().IsConcurrencyToken();
        builder.Property(p => p.DeletedAt).IsRequired(false);
        builder.Property(p => p.IsDeleted).IsRequired();
    }
}
