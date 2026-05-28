using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ValidFrom)
            .HasColumnName("valid_from")
            .HasDefaultValueSql("now() at time zone 'utc'");

        builder.Property(o => o.ValidTo)
            .HasColumnName("valid_to")
            .HasDefaultValueSql("'9999-12-31T23:59:59'::timestamp");

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Total)
            .HasPrecision(18, 2);

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(o => o.DomainEvents);
    }
}
