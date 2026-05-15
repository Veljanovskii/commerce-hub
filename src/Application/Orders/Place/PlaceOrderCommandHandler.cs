using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Place;

internal sealed class PlaceOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        bool customerExists = await dbContext.Customers
            .AnyAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (!customerExists)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound(command.CustomerId));
        }

        var productIds = command.Lines.Select(l => l.ProductId).ToList();
        Dictionary<Guid, decimal> productPrices = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price, cancellationToken);

        if (productPrices.Count != productIds.Count)
        {
            return Result.Failure<Guid>(Error.NotFound("Orders.ProductNotFound", "One or more products were not found."));
        }

        var orderLines = command.Lines.Select(l => new OrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = l.ProductId,
            Quantity = l.Quantity,
            UnitPriceAtOrder = productPrices[l.ProductId]
        }).ToList();

        Order order = new()
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAt = DateTime.UtcNow,
            Total = orderLines.Sum(l => l.Quantity * l.UnitPriceAtOrder),
            OrderLines = orderLines
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
