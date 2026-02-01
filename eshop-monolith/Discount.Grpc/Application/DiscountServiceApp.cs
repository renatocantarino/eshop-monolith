using MongoDB.Driver;

namespace Discount.Grpc.Application;

public interface IDiscountServiceApp
{
    Task<Models.Discount> GetByProduct(int productId);

    Task<Models.Discount> GetByPromoCode(string promoCode);

    Task Create(Models.Discount discount);
}

public class DiscountServiceApp(IMongoDatabase database) : IDiscountServiceApp
{
    private readonly IMongoCollection<Models.Discount> _collection = database.GetCollection<Models.Discount>("DiscountDB");

    public async Task<Models.Discount> GetByProduct(int productId)
        => await _collection.Find(d => d.ProductId == productId).FirstOrDefaultAsync();

    public async Task<Models.Discount> GetByPromoCode(string promoCode) => await _collection.Find(d => d.Code == promoCode).FirstOrDefaultAsync();

    public async Task Create(Models.Discount discount) => await _collection.InsertOneAsync(discount);
}