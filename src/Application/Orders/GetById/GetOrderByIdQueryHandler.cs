using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        OrderDetailResponse? order = await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDetailResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer!.FirstName + " " + o.Customer.LastName,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                ShippingAddress = o.ShippingAddress,
                ShippingCity = o.ShippingCity,
                ShippingCountry = o.ShippingCountry,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    ProductSku = i.Product.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    LineTotal = i.UnitPrice * i.Quantity - i.Discount
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound(query.OrderId));
        }

        return order;
    }
}
