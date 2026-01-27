using AppShared.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ordering;

public static class OrderingModule
{
    public static WebApplicationBuilder AddOrderModule(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddNpgsqlDbContext<OrderDbContext>(connectionName: "EshopDB");

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