using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Data;

public static class Extensions
{
    public static void UseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        context.Database.Migrate();

        SeedExecutor.Execute(context);
    }
}