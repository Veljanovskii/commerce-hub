using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Catalog;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ContactName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.Country)
            .HasMaxLength(100);

        builder.Ignore(s => s.DomainEvents);
    }
}
