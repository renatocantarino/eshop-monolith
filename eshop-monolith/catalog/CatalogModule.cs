using AppShared.Data;
using Catalog.Data;
using Catalog.Data.Seed;
using Catalog.Endpoints;
using Catalog.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Catalog;

public static class CatalogModule
{
    public static WebApplicationBuilder AddCatalogModule(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddNpgsqlDbContext<CatalogDbContext>(connectionName: "EshopDB");

        builder.Services.AddScoped<ISeeder, CatalogSeeder>();
        builder.Services.AddScoped<CatalogServices>();

        return builder;
    }

    public static WebApplication UseCatalogModule(this WebApplication app)
    {
        app.UseMigration<CatalogDbContext>();
        app.MapEndpointsCatalog();

        return app;
    }
}