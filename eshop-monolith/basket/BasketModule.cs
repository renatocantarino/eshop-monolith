using AppShared.Data;
using Basket.Data;
using Basket.Endpoints;
using Basket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Basket;

public static class BasketModule
{
    public static WebApplicationBuilder AddBasketModule(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddNpgsqlDbContext<BasketDbContext>(connectionName: "EshopDB");

        builder.Services.AddScoped<BasketServices>();

        return builder;
    }

    public static WebApplication UseBasketModule(this WebApplication app)
    {
        app.UseMigration<BasketDbContext>();
        app.MapEndpointsBasket();

        return app;
    }
}

/*

 dotnet ef database update -p basket -s ApiServices --context BasketDbContext

# Para Order
dotnet ef database update -p order -s ApiServices --context OrderDbContext

# Para Catalog
dotnet ef database update -p catalog -s ApiServices --context CatalogDbContext

 */