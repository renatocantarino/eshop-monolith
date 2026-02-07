using AppShared.Cqrs.Abstractions;
using AppShared.Events;
using AppShared.Events.Extensions;
using OrderingApi.Application;
using OrderingApi.Application.Consumers;
using OrderingApi.Application.Endpoints;
using OrderingApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<OrderDbContext>(connectionName: "OrderDB");

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IEventHandler<BasketCheckoutEvent>, CheckoutBasketConsumer>();

builder.Services.AddRedisStreamConsumer(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseMigration();

app.MapOrderEndpoints();

app.Run();