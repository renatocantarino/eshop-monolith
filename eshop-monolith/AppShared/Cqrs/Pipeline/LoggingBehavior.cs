using Microsoft.Extensions.Logging;

namespace AppShared.Cqrs.Pipeline;

/// <summary>
/// Pipeline behavior that logs the execution of requests.
/// Logs the start of request processing and any errors that occur.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            _logger.LogInformation("Iniciando o processamento do request {RequestName} com dados: {@Request}", requestName, request);
            var response = await next();
            return response;
        }
        catch (Exception)
        {
            _logger.LogError("Ocorreu um erro ao processar o request {RequestName} com dados: {@Request}", requestName, request);
            throw;
        }
    }
}
