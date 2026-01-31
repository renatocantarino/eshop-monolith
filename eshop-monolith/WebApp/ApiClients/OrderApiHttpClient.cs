using AppShared.Dtos;

namespace WebApp.ApiClients;

public class OrderApiHttpClient(HttpClient httpClient)
{
    public async Task<List<OrderResponse>> GetAllOrders()
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/orders");
        return response!;
    }

    public async Task<List<OrderResponse>> GetAllOrdersByUserName(string userName)
    {
        var response = await httpClient.GetFromJsonAsync<List<OrderResponse>>($"/orders/{userName}");
        return response!;
    }
}