IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("commerce-hub");

IResourceBuilder<ProjectResource> migrations = builder
    .AddProject<Projects.Web_MigrationService>("migrations")
    .WithReference(database, "Database")
    .WaitFor(database);

builder.AddProject<Projects.Web_RestApi>("web-restapi")
    .WithReference(database, "Database")
    .WaitFor(database)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.Web_GraphQLApi>("web-graphqlapi")
    .WithReference(database, "Database")
    .WaitFor(database)
    .WaitForCompletion(migrations);

await builder.Build().RunAsync();
