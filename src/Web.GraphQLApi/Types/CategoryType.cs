using Domain.Products;

namespace Web.GraphQLApi.Types;

public class CategoryType : ObjectType<Category>
{
    protected override void Configure(IObjectTypeDescriptor<Category> descriptor)
    {
        descriptor.Field(c => c.Id);
        descriptor.Field(c => c.Name);
        descriptor.Field(c => c.ParentCategoryId);
        descriptor.Field(c => c.ParentCategory).Type<CategoryType>();
        descriptor.Field(c => c.SubCategories).Type<ListType<CategoryType>>();
        descriptor.Field(c => c.Products).Ignore();
        descriptor.Field(c => c.DomainEvents).Ignore();
    }
}
