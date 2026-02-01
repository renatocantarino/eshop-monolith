using Discount.Grpc.Protos;

namespace Discount.Grpc.Mappers
{
    public static class DiscountMapper
    {
        public static CupomResponse ToProto(this Models.Discount cupom)
        {
            return new CupomResponse
            {
                Code = cupom.Code,
                ProductId = cupom.ProductId,
                Amount = double.Parse(cupom.Amount.ToString())
            };
        }
    }
}