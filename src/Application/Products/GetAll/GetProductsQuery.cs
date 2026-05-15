using Application.Abstractions.Messaging;

namespace Application.Products.GetAll;

public sealed record GetProductsQuery(Guid? CategoryId, int Page, int PageSize) : IQuery<PagedResponse<ProductResponse>>;
