using Domain.Supplies;

namespace Web.GraphQLApi.Types;

public class SupplierType : ObjectType<Supplier>
{
    protected override void Configure(IObjectTypeDescriptor<Supplier> descriptor)
    {
        descriptor.Field(s => s.Id);
        descriptor.Field(s => s.Name);
        descriptor.Field(s => s.ContactEmail);
        descriptor.Field(s => s.ContactPhone);
        descriptor.Field(s => s.StockItems).Ignore();
        descriptor.Field(s => s.SupplyOrders).Ignore();
        descriptor.Field(s => s.DomainEvents).Ignore();
    }
}
