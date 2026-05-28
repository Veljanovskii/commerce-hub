namespace Web.GraphQLApi.Types;

public class Subscription
{
    [Subscribe]
    [Topic("OrderStatus_{orderId}")]
    public string OnOrderStatusChanged(Guid orderId, [EventMessage] string status) => status;
}
