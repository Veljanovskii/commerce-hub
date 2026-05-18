using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Suppliers.GetById;

internal sealed class GetSupplierByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSupplierByIdQuery, SupplierResponse>
{
    public async Task<Result<SupplierResponse>> Handle(GetSupplierByIdQuery query, CancellationToken cancellationToken)
    {
        SupplierResponse? supplier = await context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == query.SupplierId)
            .Select(s => new SupplierResponse
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                City = s.City,
                Country = s.Country
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return Result.Failure<SupplierResponse>(SupplierErrors.NotFound(query.SupplierId));
        }

        return supplier;
    }
}
