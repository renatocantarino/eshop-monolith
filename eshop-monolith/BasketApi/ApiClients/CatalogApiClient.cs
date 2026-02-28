using AppShared.Dtos;

namespace BasketApi.ApiClients;

public class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger)
{
    // create order
    public async Task<ProductResponse> GetProductById(int id)
    {
        logger.LogInformation("GetProductById: {id}", id);

        try
        {
            var response = await httpClient.GetFromJsonAsync<ProductResponse>($"/products/{id}");
            logger.LogInformation("product founded: {id}", response?.Id);
            return response!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erro de rede ao tentar contactar a API de products.");
            throw;
        }
        catch (Exception ex)
        {
            // Log de exceções inesperadas
            logger.LogError(ex, "Erro inesperado ao processar a criação do products.");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetProductsByIds(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        logger.LogInformation("GetProductsByIds: {count} products", idList.Count);

        try
        {
            var response = await httpClient.PostAsJsonAsync("/products/by-ids", idList);
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
            return products ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erro de rede ao tentar contactar a API de products (batch).");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao processar batch de products.");
            throw;
        }
    }
}
