using MongoDB.Driver;

namespace Discount.Grpc.Mappers;

public static class DbSeeder
{
    public static async Task SeedDiscountDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var collection = database.GetCollection<Models.Discount>("DiscountDB");

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var random = new Random();
            var discounts = new List<Models.Discount>();

            for (int i = 1; i <= 5; i++)
            {
                discounts.Add(new Models.Discount
                {
                    ProductId = i,
                    Code = $"PROMO_{i}",
                    Amount = (decimal)(random.NextDouble() * (0.05 - 0.03) + 0.03)
                });
            }

            await collection.InsertManyAsync(discounts);
        }
    }
}