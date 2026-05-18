using Application.Abstractions.Messaging;
using Application.Catalog.Categories;
using Application.Catalog.Categories.Create;
using SharedKernel;
using Web.RestApi.Extensions;
using Web.RestApi.Infrastructure;

namespace Web.RestApi.Endpoints.Categories;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (
            Request request,
            ICommandHandler<CreateCategoryCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCategoryCommand
            {
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireAuthorization();
    }
}
