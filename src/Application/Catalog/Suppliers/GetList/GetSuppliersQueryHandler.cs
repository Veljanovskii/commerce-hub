using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Suppliers.GetList;

internal sealed class GetSuppliersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSuppliersQuery, List<SupplierResponse>>
{
    public async Task<Result<List<SupplierResponse>>> Handle(GetSuppliersQuery query, CancellationToken cancellationToken)
    {
        List<SupplierResponse> suppliers = await context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.CompanyName)
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
            .ToListAsync(cancellationToken);

        return suppliers;
    }
}
