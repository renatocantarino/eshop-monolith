var builder = DistributedApplication.CreateBuilder(args);

// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var eshopDb = postgres.AddDatabase("EshopDB");

var redis = builder
        .AddRedis("appcache")
        .WithRedisInsight()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

// Projects
var apiService = builder
        .AddProject<Projects.ApiServices>("apiservice")
        .WithReplicas(3)
        .WithReference(eshopDb)
        .WaitFor(eshopDb);

var webapp = builder
        .AddProject<Projects.WebApp>("webapp")
        .WithExternalHttpEndpoints()
        .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
        .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
        .WithReference(redis)
        .WaitFor(redis)
        .WithReference(apiService)
        .WaitFor(apiService);

builder.Build().Run();

//docker system prune -a --volumes