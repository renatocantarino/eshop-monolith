using AppShared.Dtos;

namespace WebApp.ApiClients;

public class YarpApiClient(HttpClient httpClient)
{
    //// Catalog Endpoints
    public async Task<List<ProductResponse>> GetProducts()
    {
        var response = await httpClient.GetFromJsonAsync<List<ProductResponse>>($"/catalog-service/products");
        return response!;
    }

    public async Task<ProductResponse> GetProductById(int id)
    {
        var response = await httpClient.GetFromJsonAsync<ProductResponse>($"/catalog-service/products/{id}");
        return response!;
    }

    public async Task<List<ProductResponse>?> SearchProducts(string query)
    {
        return await httpClient.GetFromJsonAsync<List<ProductResponse>>($"/catalog-service/products/search/{query}");
    }

    //// Basket Endpoints
    public async Task<ShoppingCartResponse> GetBasket(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<ShoppingCartResponse>($"/basket-service/basket/{userName}");
        return response!;
    }

    public async Task<ShoppingCartResponse> UpdateBasket(ShoppingCartResponse shoppingCart)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket-service/basket", shoppingCart);
        response.EnsureSuccessStatusCode();
        var updatedBasket = await response.Content.ReadFromJsonAsync<ShoppingCartResponse>();
        return updatedBasket!;
    }

    public async Task CheckoutBasket(BasketCheckoutResponse basketCheckout)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket-service/basket/checkout", basketCheckout);
        response.EnsureSuccessStatusCode();
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

    //// Ordering Endpoints
    public async Task<List<OrderResponse>> GetAllOrders()
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/ordering-service/orders");
        return response!;
    }

    public async Task<List<OrderResponse>> GetOrdersByUserName(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/ordering-service/orders/{userName}");
        return response!;
    }
}