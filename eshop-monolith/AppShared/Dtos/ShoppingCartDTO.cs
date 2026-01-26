namespace AppShared.Dtos;

public class ShoppingCartDTO
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public List<ShoppingCartItemDTO> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}