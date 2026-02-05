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

var rabbitMQ = builder
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
                        .WithReference(rabbitMQ)
                        .WaitFor(catalogDB)
                        .WaitFor(rabbitMQ);

var ordering = builder
                    .AddProject<Projects.OrderingApi>("ordering")
                    .WithReference(orderDb)
                    .WithReference(rabbitMQ)
                    .WaitFor(orderDb)
                    .WaitFor(rabbitMQ);

var basket = builder
                    .AddProject<Projects.BasketApi>("basket")
                    .WithReference(cache)
                    .WithReference(rabbitMQ)
                    .WithReference(discountApi) // Permite que o Basket encontre a URL do gRPC
                    .WaitFor(cache)
                    .WaitFor(discountApi)
                    .WaitFor(rabbitMQ);

var gateway = builder.AddProject<Projects.YarpGateway>("yarpgateway")
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