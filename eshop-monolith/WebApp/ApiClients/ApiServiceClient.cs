using AppShared.Dtos;

namespace WebApp.ApiClients;

public class ApiServiceClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<ProductDTO>> GetProducts(CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<List<ProductDTO>>($"/products", ct);
        return response?.AsReadOnly() ?? (IReadOnlyCollection<ProductDTO>)[];
    }

    public async Task<ShoppingCartDTO> GetBasket(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<ShoppingCartDTO>($"/apiservice/basket/{userName}");
        return response!;
    }

    public async Task<OrderDTO> CheckoutBasket(OrderDTO checkoutOrder)
    {
        var response = await httpClient.PostAsJsonAsync($"/apiservice/basket/checkout", checkoutOrder);
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderDTO>();
        return order!;
    }

    public async Task<List<OrderDTO>> GetAllOrders()
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderDTO>>($"/apiservice/orders");
        return response!;
    }

    public async Task<ShoppingCartDTO> UpdateBasket(ShoppingCartDTO shoppingCart)
    {
        var response = await httpClient.PostAsJsonAsync($"/apiservice/basket", shoppingCart);
        response.EnsureSuccessStatusCode();
        var updatedBasket = await response.Content.ReadFromJsonAsync<ShoppingCartDTO>();
        return updatedBasket!;
    }

    public async Task<ShoppingCartDTO> LoadUserBasket()
    {
        // Get Basket If Not Exist Create New Basket with Default Logged In User Name: swn
        var userName = "swn";
        ShoppingCartDTO basket;

        try
        {
            basket = await GetBasket(userName);
        }
        catch
        {
            basket = new ShoppingCartDTO
            {
                UserName = userName,
                Items = []
            };
        }

        return basket;
    }
}