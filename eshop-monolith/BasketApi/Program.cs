using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Extensions;
using AppShared.Messaging;
using BasketApi.ApiClients;
using BasketApi.Application;
using BasketApi.Application.useCases.queries;
using BasketApi.Endpoints;
using Discount.Grpc.Protos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache(connectionName: "cache");

// Add Redis connection for Event Bus (Streams)
builder.AddRedisClient(connectionName: "cache");

// Configure Redis Messaging options
builder.Services.Configure<RedisMessagingOptions>(
    builder.Configuration.GetSection(RedisMessagingOptions.SectionName));

// Register Event Bus
builder.Services.AddSingleton<IEventBus, RedisEventBus>();

// Add application services

builder.Services.AddGrpcClient<DiscountService.DiscountServiceClient>(opt => opt.Address = new Uri("https://localhost:9987"));

// 2. DEPOIS: Registre os seus serviços que dependem do gRPC
builder.Services.AddScoped<DiscountGrpcService>();
builder.Services.AddScoped<IBasketServiceApp, BasketServiceApp>();

// Add CQRS Mediator
builder.Services.AddRaptorMediator();

builder.Services.Scan(scan => scan
    .FromAssembliesOf(typeof(GetByUserName))
    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime()
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapEndpointsBasket();

app.Run();