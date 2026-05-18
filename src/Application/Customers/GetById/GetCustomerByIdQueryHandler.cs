using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.GetById;

internal sealed class GetCustomerByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        CustomerResponse? customer = await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == query.CustomerId)
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                CreatedAt = c.CreatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound(query.CustomerId));
        }

        return customer;
    }
}
