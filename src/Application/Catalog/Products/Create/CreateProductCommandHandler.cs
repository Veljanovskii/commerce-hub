using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Products.Create;

internal sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        bool skuExists = await context.Products
            .AnyAsync(p => p.Sku == command.Sku, cancellationToken);

        if (skuExists)
        {
            return Result.Failure<Guid>(ProductErrors.SkuAlreadyExists(command.Sku));
        }

        bool categoryExists = await context.Categories
            .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<Guid>(CategoryErrors.NotFound(command.CategoryId));
        }

        bool supplierExists = await context.Suppliers
            .AnyAsync(s => s.Id == command.SupplierId, cancellationToken);

        if (!supplierExists)
        {
            return Result.Failure<Guid>(SupplierErrors.NotFound(command.SupplierId));
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Sku = command.Sku,
            Description = command.Description,
            UnitPrice = command.UnitPrice,
            StockQuantity = command.StockQuantity,
            CategoryId = command.CategoryId,
            SupplierId = command.SupplierId,
            CreatedAt = dateTimeProvider.UtcNow
        };

        product.Raise(new ProductCreatedDomainEvent(product.Id));

        context.Products.Add(product);

        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
