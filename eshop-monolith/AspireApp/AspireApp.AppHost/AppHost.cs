var builder = DistributedApplication.CreateBuilder(args);

// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var eshopDb = postgres.AddDatabase("EshopDB");

// Projects
var apiService = builder
        .AddProject<Projects.ApiServices>("apiservice")
        .WithReference(eshopDb)
        .WaitFor(eshopDb);

var webapp = builder
        .AddProject<Projects.WebApp>("webapp")
        .WithExternalHttpEndpoints()
        .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
        .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
        .WithReference(apiService)
        .WaitFor(apiService);

builder.Build().Run();

//docker system prune -a --volumes