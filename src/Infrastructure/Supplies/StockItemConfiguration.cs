using Domain.Supplies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Supplies;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ValidFrom)
            .HasColumnName("valid_from")
            .HasDefaultValueSql("now() at time zone 'utc'");

        builder.Property(s => s.ValidTo)
            .HasColumnName("valid_to")
            .HasDefaultValueSql("'9999-12-31T23:59:59'::timestamp");

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockItems)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Supplier)
            .WithMany(sup => sup.StockItems)
            .HasForeignKey(s => s.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ProductId, s.SupplierId }).IsUnique();

        builder.Ignore(s => s.DomainEvents);
    }
}
