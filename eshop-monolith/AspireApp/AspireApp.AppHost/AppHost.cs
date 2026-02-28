var builder = DistributedApplication.CreateBuilder(args);

// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithImageTag("18.3-alpine3.23")
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogDB = postgres.AddDatabase("CatalogDB");
var orderDb = postgres.AddDatabase("OrderDB");

var cache = builder
        .AddRedis("cache")
        //.WithRedisInsight()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var mongo = builder
        .AddMongoDB("mongodb")
        //.WithMongoExpress()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("DiscountDB");

var rabbitmq = builder
        .AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

//Application Services

var discountApi = builder.AddProject<Projects.Discount_Grpc>("discount-grpc")
                             .WithHttpEndpoint(port: 9988, name: "grpc")
                             .WithHttpsEndpoint(port: 9987, name: "grpcx")
                             .WithReference(mongodb)
                             .WaitFor(mongodb);

var catalog = builder.AddProject<Projects.CatalogApi>("catalog")
                        .WithReference(catalogDB)
                        .WithReference(rabbitmq)
                        .WaitFor(catalogDB)
                        .WaitFor(rabbitmq);

var ordering = builder
                    .AddProject<Projects.OrderingApi>("ordering")
                    .WithReference(orderDb)
                    .WithReference(rabbitmq)
                    .WaitFor(orderDb)
                    .WaitFor(rabbitmq);

var basket = builder
                    .AddProject<Projects.BasketApi>("basket")
                    .WithReference(cache)
                    .WithReference(rabbitmq)
                    .WithReference(discountApi)
                    .WithReference(catalog)
                    .WaitFor(cache)
                    .WaitFor(discountApi)
                    .WaitFor(rabbitmq)
                    .WaitFor(catalog);

var gateway = builder.AddProject<Projects.YarpGateway>("yarpapigateway")
                        .WithReference(catalog)
                        .WithReference(basket)
                        .WithReference(ordering)
                        .WaitFor(catalog)
                        .WaitFor(basket)
                        .WaitFor(ordering);

var webapp = builder
                        .AddProject<Projects.WebApp>("webapp")
                        .WithExternalHttpEndpoints()
                        .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
                        .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
                        .WithReference(gateway)
                        .WaitFor(gateway);

builder.Build().Run();