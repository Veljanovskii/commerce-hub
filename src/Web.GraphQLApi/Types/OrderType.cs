using Domain.Orders;

namespace Web.GraphQLApi.Types;

public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor.Field(o => o.Id);
        descriptor.Field(o => o.CustomerId);
        descriptor.Field(o => o.Customer).Type<CustomerType>();
        descriptor.Field(o => o.Status);
        descriptor.Field(o => o.PlacedAt);
        descriptor.Field(o => o.Total);
        descriptor.Field(o => o.OrderLines).Type<ListType<OrderLineType>>();
        descriptor.Field(o => o.DomainEvents).Ignore();
    }
}
