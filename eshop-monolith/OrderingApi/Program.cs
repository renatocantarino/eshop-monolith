using AppShared.Messaging;
using OrderingApi.Application;
using OrderingApi.Application.Endpoints;
using OrderingApi.Data;
using OrderingApi.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<OrderDbContext>(connectionName: "OrderDB");

// Add Redis connection for Event Bus (Streams)
builder.AddRedisClient(connectionName: "cache");

// Configure Redis Messaging options
builder.Services.Configure<RedisMessagingOptions>(
    builder.Configuration.GetSection(RedisMessagingOptions.SectionName));

// Register services
builder.Services.AddScoped<OrderService>();

// Register the Redis Stream Worker as a hosted service
builder.Services.AddHostedService<RedisStreamWorker>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseMigration();

app.MapOrderEndpoints();

app.Run();
