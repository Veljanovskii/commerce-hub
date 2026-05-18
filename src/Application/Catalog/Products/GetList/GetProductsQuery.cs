using Application.Abstractions.Messaging;

namespace Application.Catalog.Products.GetList;

public sealed class GetProductsQuery : IQuery<List<ProductListResponse>>
{
    public Guid? CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
