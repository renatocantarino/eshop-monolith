using BasketApi.Application.Models;
using Discount.Grpc.Protos;

namespace BasketApi.ApiClients;

public class DiscountGrpcService
{
    private readonly DiscountService.DiscountServiceClient _discountServiceClient;

    public DiscountGrpcService(DiscountService.DiscountServiceClient discountServiceClient)
    {
        _discountServiceClient = discountServiceClient;
    }

    public async Task<Cupom> GetDiscountAsync(int productId)
    {
        var request = new GetDiscountRequest { ProductId = productId };
        var cp = await _discountServiceClient.GetDiscountAsync(request);

        return new Cupom
        {
            ProductId = cp.ProductId,
            Code = cp.Code,
            Amount = (decimal)cp.Amount
        };
    }
}