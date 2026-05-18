using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Suppliers.Create;

internal sealed class CreateSupplierCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateSupplierCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        bool emailExists = await context.Suppliers
            .AnyAsync(s => s.Email == command.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(SupplierErrors.EmailAlreadyExists(command.Email));
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = command.CompanyName,
            ContactName = command.ContactName,
            Email = command.Email,
            Phone = command.Phone,
            Address = command.Address,
            City = command.City,
            Country = command.Country
        };

        supplier.Raise(new SupplierCreatedDomainEvent(supplier.Id));

        context.Suppliers.Add(supplier);

        await context.SaveChangesAsync(cancellationToken);

        return supplier.Id;
    }
}
