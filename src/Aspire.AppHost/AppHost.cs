IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("commerce-hub");

builder.AddProject<Projects.Web_RestApi>("web-restapi")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.Web_GraphQLApi>("web-graphqlapi")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync();
