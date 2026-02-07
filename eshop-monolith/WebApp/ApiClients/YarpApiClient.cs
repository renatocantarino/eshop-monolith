using AppShared.Dtos;
using StackExchange.Redis;
using WebApp.Components.Pages;

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

public class BasketApiClient(HttpClient httpClient)
{
    public async Task<ShoppingCartResponse> GetBasket(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<ShoppingCartResponse>($"/basket/{userName}");
        return response!;
    }

    public async Task<ShoppingCartResponse> UpdateBasket(ShoppingCartResponse shoppingCart)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket", shoppingCart);
        response.EnsureSuccessStatusCode();
        var updatedBasket = await response.Content.ReadFromJsonAsync<ShoppingCartResponse>();
        return updatedBasket!;
    }

    public async Task CheckoutBasket(BasketCheckoutResponse basketCheckout)
    {
        var response = await httpClient.PostAsJsonAsync($"/basket/checkout", basketCheckout);
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
            }
            ;
        }

        return basket;
    }
}

public class CatalogApiClient(HttpClient httpClient)
{
    public async Task<List<ProductResponse>> GetProducts()
    {
        var response = await httpClient.GetFromJsonAsync<List<ProductResponse>>($"/products");
        return response!;
    }

    public async Task<ProductResponse> GetProductById(int id)
    {
        var response = await httpClient.GetFromJsonAsync<ProductResponse>($"/products/{id}");
        return response!;
    }

    public async Task<List<ProductResponse>?> SearchProducts(string query)
    {
        return await httpClient.GetFromJsonAsync<List<ProductResponse>>($"/products/search/{query}");
    }
}

public class OrderingApiClient(HttpClient httpClient)
{
    public async Task<List<OrderResponse>> GetAllOrders()
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/orders");
        return response!;
    }

    public async Task<List<OrderResponse>> GetOrdersByUserName(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/orders/{userName}");
        return response!;
    }
}