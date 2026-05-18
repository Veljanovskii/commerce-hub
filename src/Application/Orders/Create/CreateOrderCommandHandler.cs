using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Domain.Customers;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Customer? customer = await context.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound(command.CustomerId));
        }

        if (command.Items.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.EmptyOrder());
        }

        Guid[] productIds = command.Items.Select(i => i.ProductId).ToArray();
        List<Product> products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            OrderDate = dateTimeProvider.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = command.ShippingAddress,
            ShippingCity = command.ShippingCity,
            ShippingCountry = command.ShippingCountry
        };

        decimal totalAmount = 0;

        foreach (CreateOrderItemDto itemDto in command.Items)
        {
            Product? product = products.SingleOrDefault(p => p.Id == itemDto.ProductId);

            if (product is null)
            {
                return Result.Failure<Guid>(ProductErrors.NotFound(itemDto.ProductId));
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                return Result.Failure<Guid>(ProductErrors.InsufficientStock(
                    product.Id, itemDto.Quantity, product.StockQuantity));
            }

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.UnitPrice,
                Discount = itemDto.Discount
            };

            decimal lineTotal = orderItem.UnitPrice * orderItem.Quantity - orderItem.Discount;
            totalAmount += lineTotal;

            order.Items.Add(orderItem);

            product.StockQuantity -= itemDto.Quantity;
        }

        order.TotalAmount = totalAmount;

        order.Raise(new OrderPlacedDomainEvent(order.Id));

        context.Orders.Add(order);

        await context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
