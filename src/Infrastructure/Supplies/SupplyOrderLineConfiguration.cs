using Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Supplies;

internal sealed class SupplyOrderLineConfiguration : IEntityTypeConfiguration<SupplyOrderLine>
{
    public void Configure(EntityTypeBuilder<SupplyOrderLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.SupplyOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(l => l.SupplyOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
