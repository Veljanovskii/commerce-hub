using Application.Abstractions.Messaging;

namespace Application.Catalog.Products.GetById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDetailResponse>;
