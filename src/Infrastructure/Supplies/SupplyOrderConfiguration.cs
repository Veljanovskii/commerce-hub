using Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Supplies;

internal sealed class SupplyOrderConfiguration : IEntityTypeConfiguration<SupplyOrder>
{
    public void Configure(EntityTypeBuilder<SupplyOrder> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(o => o.Supplier)
            .WithMany(s => s.SupplyOrders)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(o => o.DomainEvents);
    }
}
