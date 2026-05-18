using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Login;

internal sealed class LoginCustomerCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider)
    : ICommandHandler<LoginCustomerCommand, string>
{
    public async Task<Result<string>> Handle(LoginCustomerCommand command, CancellationToken cancellationToken)
    {
        Customer? customer = await context.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Email == command.Email, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<string>(CustomerErrors.NotFoundByEmail(command.Email));
        }

        bool passwordValid = passwordHasher.Verify(command.Password, customer.PasswordHash);

        if (!passwordValid)
        {
            return Result.Failure<string>(CustomerErrors.InvalidCredentials());
        }

        string token = tokenProvider.Create(customer);

        return token;
    }
}
