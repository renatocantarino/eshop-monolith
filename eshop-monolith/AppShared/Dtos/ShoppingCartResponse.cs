namespace AppShared.Dtos;

public class ShoppingCartResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public List<ShoppingCartItemResponse> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}