using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Products.UpdateStock;

internal sealed class UpdateStockCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateStockCommand>
{
    public async Task<Result> Handle(UpdateStockCommand command, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .SingleOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        product.StockQuantity = command.Quantity;
        product.Raise(new StockUpdatedDomainEvent(product.Id, command.Quantity));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
