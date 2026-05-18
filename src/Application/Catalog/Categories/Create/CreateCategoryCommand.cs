using Application.Abstractions.Messaging;

namespace Application.Catalog.Categories.Create;

public sealed class CreateCategoryCommand : ICommand<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}
