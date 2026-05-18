using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Catalog.Categories.Create;

internal sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        bool nameExists = await context.Categories
            .AnyAsync(c => c.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(CategoryErrors.NameAlreadyExists(command.Name));
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ParentCategoryId = command.ParentCategoryId
        };

        context.Categories.Add(category);

        await context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
