var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite(
    "database",
    builder.Configuration["Database:path"],
    builder.Configuration["Database:fileName"]);
    // .WithSqliteWeb(); // optionally add web admin UI (requires Docker/podman)

var webapi = builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(sqlite);

var frontend = builder.AddNpmApp("frontend", "../Frontend")
    .WithReference(webapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
