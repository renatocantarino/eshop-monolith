using Discount.Grpc.Application;
using Discount.Grpc.Mappers;
using Discount.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("DiscountDB");

builder.Services.AddScoped<IDiscountServiceApp, DiscountServiceApp>();
builder.Services.AddGrpc();

var app = builder.Build();

await app.Services.SeedDiscountDataAsync();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();