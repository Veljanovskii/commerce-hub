using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Register;

internal sealed class RegisterCustomerCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterCustomerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        bool emailExists = await context.Customers
            .AnyAsync(c => c.Email == command.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(CustomerErrors.EmailAlreadyExists(command.Email));
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password),
            Phone = command.Phone,
            Address = command.Address,
            City = command.City,
            Country = command.Country,
            CreatedAt = dateTimeProvider.UtcNow
        };

        customer.Raise(new CustomerRegisteredDomainEvent(customer.Id));

        context.Customers.Add(customer);

        await context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
