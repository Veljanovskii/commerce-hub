IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("commerce-hub");

builder.AddProject<Projects.Web_RestApi>("web-restapi")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync();
