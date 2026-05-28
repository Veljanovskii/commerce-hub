using Application.Abstractions.Data;
using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.Types;

public class Mutation
{
    public async Task<Order?> PlaceOrder(
        PlaceOrderInput input,
        [Service] IApplicationDbContext dbContext,
        [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken)
    {
        bool customerExists = await dbContext.Customers
            .AnyAsync(c => c.Id == input.CustomerId, cancellationToken);

        if (!customerExists)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage($"Customer with id '{input.CustomerId}' was not found.")
                    .SetCode("CUSTOMER_NOT_FOUND")
                    .Build());
        }

        var productIds = input.Lines.Select(l => l.ProductId).ToList();
        Dictionary<Guid, decimal> productPrices = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Price, cancellationToken);

        if (productPrices.Count != productIds.Count)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("One or more products were not found.")
                    .SetCode("PRODUCT_NOT_FOUND")
                    .Build());
        }

        var orderLines = input.Lines.Select(l => new OrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = l.ProductId,
            Quantity = l.Quantity,
            UnitPriceAtOrder = productPrices[l.ProductId]
        }).ToList();

        Order order = new()
        {
            Id = Guid.NewGuid(),
            CustomerId = input.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAt = DateTime.UtcNow,
            Total = orderLines.Sum(l => l.Quantity * l.UnitPriceAtOrder),
            OrderLines = orderLines
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventSender.SendAsync($"OrderStatus_{order.Id}", order.Status.ToString(), cancellationToken);

        return order;
    }
}

public sealed record PlaceOrderInput(Guid CustomerId, List<PlaceOrderLineInput> Lines);

public sealed record PlaceOrderLineInput(Guid ProductId, int Quantity);
