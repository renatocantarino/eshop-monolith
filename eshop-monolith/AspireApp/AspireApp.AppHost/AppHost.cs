var builder = DistributedApplication.CreateBuilder(args);

// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogDB = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderDB");

var cache = builder
        .AddRedis("cache")
        .WithRedisInsight()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalog = builder.AddProject<Projects.CatalogApi>("catalog")
        .WithReference(catalogDB)
        .WaitFor(catalogDB);

var basket = builder
        .AddProject<Projects.BasketApi>("basket")
        .WithReference(cache)
        .WaitFor(cache);

var ordering = builder
        .AddProject<Projects.OrderingApi>("ordering")
        .WithReference(orderDb)
        .WaitFor(orderDb);

var webapp = builder
        .AddProject<Projects.WebApp>("webapp")
        .WithExternalHttpEndpoints()
        .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
        .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
        .WithReference(catalog)
        .WithReference(basket)
        .WithReference(ordering)
        .WaitFor(catalog)
        .WaitFor(basket)
        .WaitFor(ordering);

builder.Build().Run();