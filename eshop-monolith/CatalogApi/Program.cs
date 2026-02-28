using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Extensions;
using AppShared.Dtos;
using AppShared.Events.Extensions;
using CatalogApi.Application;
using CatalogApi.Application.Endpoints;
using CatalogApi.Application.useCases;
using CatalogApi.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>(connectionName: "CatalogDB");
// Add services to the container.

builder.Services.AddScoped<ICatalogServiceApp, CatalogServiceApp>();

builder.Services.AddRaptorMediator();
builder.Services.AddMassTransitConfig(Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IQueryHandler<GetAllQuery, IReadOnlyCollection<ProductResponse>>, GetAllQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetByIdQuery, ProductResponse>, GetByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetByIdsQuery, IReadOnlyCollection<ProductResponse>>, GetByIdsQueryHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseMigration();
app.MapProductEndpoints();

app.Run();