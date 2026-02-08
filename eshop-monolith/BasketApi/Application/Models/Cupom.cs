namespace BasketApi.Application.Models;

public class Cupom
{
    public int ProductId { get; set; }
    public string Code { get; set; }

    public decimal Amount { get; set; }
}