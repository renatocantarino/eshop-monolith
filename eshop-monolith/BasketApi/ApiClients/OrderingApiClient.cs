using AppShared.Dtos;

namespace BasketApi.ApiClients;

public class OrderingApiClient(HttpClient httpClient, ILogger<OrderingApiClient> logger)
{
    // create order
    public async Task<bool> CreateOrder(OrderResponse orderDto)
    {
        logger.LogInformation("Tentando criar pedido para o cliente: {UserName}", orderDto.UserName);

        try
        {
            var response = await httpClient.PostAsJsonAsync("/orders", orderDto);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Pedido criado com sucesso. Status: {StatusCode}", response.StatusCode);
                return true;
            }

            // Log de erro quando a API responde, mas com erro (ex: 400 ou 500)
            logger.LogWarning("Falha ao criar pedido. Status: {StatusCode}. Motivo: {Reason}",
                response.StatusCode, response.ReasonPhrase);

            return false;
        }
        catch (HttpRequestException ex)
        {
            // Log de erro crítico (falha de conectividade)
            logger.LogError(ex, "Erro de rede ao tentar contactar a API de pedidos.");
            return false;
        }
        catch (Exception ex)
        {
            // Log de exceções inesperadas
            logger.LogError(ex, "Erro inesperado ao processar a criação do pedido.");
            throw;
        }
    }
}