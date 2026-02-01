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

var mongo = builder
        .AddMongoDB("mongodb")
        .WithMongoExpress()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("DiscountDB");

var catalog = builder.AddProject<Projects.CatalogApi>("catalog")
        .WithReference(catalogDB)
        .WaitFor(catalogDB);

var ordering = builder
        .AddProject<Projects.OrderingApi>("ordering")
        .WithReference(orderDb)
        .WaitFor(orderDb);

var discountApi = builder.AddProject<Projects.Discount_Grpc>("discount-grpc")
         .WithHttpEndpoint(port: 9988, name: "grpc")
         .WithHttpsEndpoint(port: 9987, name: "grpcx")
         .WithReference(mongodb)
         .WaitFor(mongodb);

var basket = builder
    .AddProject<Projects.BasketApi>("basket")
    .WithReference(cache)
    .WithReference(discountApi) // Permite que o Basket encontre a URL do gRPC
    .WaitFor(cache)
    .WaitFor(discountApi);

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