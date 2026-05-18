using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.GraphQLApi.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();

        SeedDataGenerator seedDataGenerator =
            scope.ServiceProvider.GetRequiredService<SeedDataGenerator>();

        seedDataGenerator.GenerateAsync().GetAwaiter().GetResult();
    }
}
