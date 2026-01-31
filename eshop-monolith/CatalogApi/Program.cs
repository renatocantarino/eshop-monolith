using AppShared.Cqrs.Extensions;
using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using CatalogApi.Application;
using CatalogApi.Application.Endpoints;
using CatalogApi.Application.useCases;
using CatalogApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>(connectionName: "CatalogDB");
// Add services to the container.

builder.Services.AddScoped<ICatalogServiceApp, CatalogServiceApp>();

builder.Services.AddRaptorMediator();
builder.Services.AddScoped<IQueryHandler<GetAllQuery, IReadOnlyCollection<ProductResponse>>, GetAllQueryHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseMigration();
app.MapProductEndpoints();

app.Run();