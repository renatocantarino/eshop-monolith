using AppShared.Dtos;

namespace WebApp.ApiClients;

public class BasketApiHttpClient(HttpClient httpClient)
{
    public async Task<ShoppingCartResponse> GetBasket(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<ShoppingCartResponse>($"/basket/{userName}");
        return response!;
    }

    public async Task CheckoutBasket(BasketCheckoutResponse checkoutOrder)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket/checkout", checkoutOrder);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ShoppingCartResponse> UpdateBasket(ShoppingCartResponse shoppingCart)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket", shoppingCart);
        response.EnsureSuccessStatusCode();
        var updatedBasket = await response.Content.ReadFromJsonAsync<ShoppingCartResponse>();
        return updatedBasket!;
    }

    public async Task<ShoppingCartResponse> LoadUserBasket()
    {
        // Get Basket If Not Exist Create New Basket with Default Logged In User Name: swn
        var userName = "swn";
        ShoppingCartResponse basket;

        try
        {
            basket = await GetBasket(userName);
        }
        catch
        {
            basket = new ShoppingCartResponse
            {
                UserName = userName,
                Items = []
            };
        }

        return basket;
    }
}
