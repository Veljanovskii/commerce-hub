using System.Diagnostics;
using Infrastructure.Database;
using Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Web.MigrationService;

public sealed class MigrationWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<MigrationWorker> logger) : BackgroundService
{
    public const string ActivitySourceName = "CommerceHub.Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using Activity? activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync(stoppingToken);

            logger.LogInformation("Seeding database...");
            DataSeeder.SeedData(serviceProvider);

            logger.LogInformation("Database migration and seeding complete.");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw new InvalidOperationException("Database migration or seeding failed.", ex);
        }

        hostApplicationLifetime.StopApplication();
    }
}
