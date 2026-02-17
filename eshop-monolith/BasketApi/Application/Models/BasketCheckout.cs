namespace BasketApi.Application.Models;

public class BasketCheckout
{
    public string UserName { get; set; } = default!;
    public decimal TotalPrice { get; set; }

    // Address

    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string EmailAddress { get; set; } = default!;

    public string AddressLine { get; set; } = default!;

    public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

    public int ShoppingCartId { get; set; } = default!;
}