using OrderingApi.Application;
using OrderingApi.Application.Endpoints;
using OrderingApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<OrderDbContext>(connectionName: "OrderDB");

builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseMigration();

app.MapOrderEndpoints();

app.Run();