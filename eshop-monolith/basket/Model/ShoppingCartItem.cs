namespace Basket.Model;

public class ShoppingCartItem
{
    public int Id { get; set; }
    public int ShoppingCartId { get; set; }
    public int Quantity { get; set; } = default!;
    public string Color { get; set; } = default!;
    public int ProductId { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public string ProductName { get; set; } = default!;

    public ShoppingCart? ShoppingCart { get; set; }
}
