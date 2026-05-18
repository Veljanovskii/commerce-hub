using SharedKernel;

namespace Domain.Catalog;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) => Error.NotFound(
        "Categories.NotFound",
        $"The category with the Id = '{categoryId}' was not found");

    public static Error NameAlreadyExists(string name) => Error.Conflict(
        "Categories.NameAlreadyExists",
        $"A category with the name = '{name}' already exists");
}
