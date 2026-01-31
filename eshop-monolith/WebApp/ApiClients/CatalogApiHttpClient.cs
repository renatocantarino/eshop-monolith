using AppShared.Dtos;

namespace WebApp.ApiClients;

public class CatalogApiHttpClient(HttpClient httpClient)
{
    public async Task<List<ProductResponse>> GetProducts(CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<List<ProductResponse>>($"/products", ct);
        return response!;
    }
}