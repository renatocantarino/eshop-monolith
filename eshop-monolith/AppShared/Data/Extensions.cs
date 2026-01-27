using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Data;

public static class Extensions
{
    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app)
        where TContext : DbContext
    {
        MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();
        SeedAsync(app.ApplicationServices).GetAwaiter().GetResult();

        return app;
    }

    private static async Task SeedAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<ISeeder>();
        foreach (var seeder in seeders)
        {
            await seeder.ExecuteAsync();
        }
    }

    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider provider) where TContext : DbContext
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }
}