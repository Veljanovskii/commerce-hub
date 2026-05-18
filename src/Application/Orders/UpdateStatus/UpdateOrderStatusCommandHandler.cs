using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.UpdateStatus;

internal sealed class UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .SingleOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.AlreadyCancelled(command.OrderId));
        }

        OrderStatus oldStatus = order.Status;
        order.Status = command.NewStatus;

        order.Raise(new OrderStatusChangedDomainEvent(order.Id, oldStatus, command.NewStatus));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
