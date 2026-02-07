using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Extensions;
using AppShared.Events.Extensions;
using BasketApi.ApiClients;
using BasketApi.Application;
using BasketApi.Application.useCases.queries;
using BasketApi.Endpoints;
using Discount.Grpc.Protos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisClient("cache");
builder.AddRedisDistributedCache(connectionName: "cache");

// Add application services

builder.Services.AddGrpcClient<DiscountService.DiscountServiceClient>(opt => opt.Address = new Uri("https://localhost:9987"));

// 2. DEPOIS: Registre os seus serviços que dependem do gRPC
builder.Services.AddScoped<DiscountGrpcService>();
builder.Services.AddScoped<IBasketServiceApp, BasketServiceApp>();

builder.Services.AddHttpClient<OrderingApiClient>(client =>
{
    client.BaseAddress = new("https+http://ordering");
}).AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    });

// Add CQRS Mediator
builder.Services.AddRaptorMediator()
                 .AddRedisEvents();

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