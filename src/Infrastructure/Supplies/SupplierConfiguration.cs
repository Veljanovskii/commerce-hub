using Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Supplies;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(200);

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(50);

        builder.Ignore(s => s.DomainEvents);
    }
}
