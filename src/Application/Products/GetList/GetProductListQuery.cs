using Application.Abstractions.Messaging;

namespace Application.Products.GetList;

public sealed record GetProductListQuery(
    Guid? CategoryId,
    int Page,
    int PageSize) : IQuery<List<ProductResponse>>;
