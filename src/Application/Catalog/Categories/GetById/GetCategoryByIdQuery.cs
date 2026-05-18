using Application.Abstractions.Messaging;

namespace Application.Catalog.Categories.GetById;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<CategoryResponse>;
