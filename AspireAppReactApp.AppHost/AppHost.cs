var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlite("db").WithSqliteWeb();

var apiService = builder.AddProject<Projects.AspireAppReactApp_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db)
    .WithHttpHealthCheck("/health");


builder.AddViteApp(name: "todo-frontend", workingDirectory: "../todo-frontend")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithNpmPackageInstallation();

builder.AddNpmApp(name: "todo-frontend-angular", workingDirectory: "../todo-angualr-app")
.WithReference(apiService)
.WaitFor(apiService)
.WithHttpEndpoint(env: "PORT")
// optional
//.PublishAsDockerFile()
    .WithNpmPackageInstallation();

builder.AddProject<Projects.AspireAppReactApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
