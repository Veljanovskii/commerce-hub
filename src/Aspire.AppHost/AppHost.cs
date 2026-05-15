IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("commerce-hub");

IResourceBuilder<ContainerResource> seq = builder
    .AddContainer("seq", "datalust/seq", "latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithHttpEndpoint(port: 5341, targetPort: 5341, name: "ingestion")
    .WithHttpEndpoint(port: 8081, targetPort: 80, name: "ui");

builder.AddProject<Projects.Web_RestApi>("web-restapi")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WithEnvironment("SEQ_URL", seq.GetEndpoint("ingestion"))
    .WaitFor(database);

builder.AddProject<Projects.Web_GraphQLApi>("web-graphqlapi")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithReference(database)
    .WithEnvironment("SEQ_URL", seq.GetEndpoint("ingestion"))
    .WaitFor(database);

await builder.Build().RunAsync();
