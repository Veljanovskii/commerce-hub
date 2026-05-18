using SharedKernel;

namespace Domain.Catalog;

public sealed class Category : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}
