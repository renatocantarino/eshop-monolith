using Discount.Grpc.Application;
using Discount.Grpc.Mappers;
using Discount.Grpc.Protos;
using Grpc.Core;

namespace Discount.Grpc.Services;

public class DiscountService : Protos.DiscountService.DiscountServiceBase
{
    private readonly IDiscountServiceApp _discountServiceApp;

    public DiscountService(IDiscountServiceApp discountServiceApp)
    {
        _discountServiceApp = discountServiceApp;
    }

    public override async Task<CupomResponse> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var cupom = await _discountServiceApp.GetByProduct(request.ProductId);
        if (cupom is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductId={request.ProductId} is not found."));

        return cupom.ToProto();
    }
}