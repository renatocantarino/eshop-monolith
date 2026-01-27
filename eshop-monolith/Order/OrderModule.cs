using AppShared.Data;
using Orders.Data;
using Orders.Data.Seed;
using Orders.Endpoints;
using Orders.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Orders;

public static class OrderModule
{
    public static WebApplicationBuilder AddOrderModule(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddNpgsqlDbContext<OrderDbContext>(connectionName: "EshopDB");

        builder.Services.AddScoped<ISeeder, OrderSeeder>();
        builder.Services.AddScoped<OrderServices>();

        return builder;
    }

    public static WebApplication UseOrderModule(this WebApplication app)
    {
        app.UseMigration<OrderDbContext>();
        app.MapEndpointsOrder();

        return app;
    }
}
