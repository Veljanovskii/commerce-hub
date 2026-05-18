using Application.Abstractions.Messaging;

namespace Application.Catalog.Categories.GetList;

public sealed record GetCategoriesQuery() : IQuery<List<CategoryResponse>>;
