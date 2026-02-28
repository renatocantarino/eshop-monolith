using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Extensions;
using AppShared.Events.Extensions;
using BasketApi.ApiClients;
using BasketApi.Application;
using BasketApi.Application.useCases.queries;
using BasketApi.Endpoints;
using Discount.Grpc.Protos;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache(connectionName: "cache");

// Add application services

builder.Services.AddGrpcClient<DiscountService.DiscountServiceClient>(opt => opt.Address = new Uri("https://localhost:9987"));

builder.Services.AddScoped<DiscountGrpcService>();
builder.Services.AddScoped<IBasketServiceApp, BasketServiceApp>();

builder.Services.AddHttpClient<CatalogApiClient>(client =>
{
    client.BaseAddress = new("https+http://catalog");
}).AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    });

// Add CQRS Mediator
builder.Services.AddRaptorMediator();

builder.Services.AddMassTransitConfig(Assembly.GetExecutingAssembly());

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

app.UseHttpsRedirection();

app.MapEndpointsBasket();

app.Run();