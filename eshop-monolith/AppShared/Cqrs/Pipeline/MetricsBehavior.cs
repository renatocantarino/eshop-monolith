using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AppShared.Cqrs.Pipeline;

/// <summary>
/// Pipeline behavior that measures and logs the execution time of requests.
/// Useful for performance monitoring and identifying slow operations.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<MetricsBehavior<TRequest, TResponse>> _logger;

    public MetricsBehavior(ILogger<MetricsBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var monitor = Stopwatch.StartNew();

        try
        {
            var response = await next();
            monitor.Stop();
            _logger.LogInformation("Request {RequestName} processado em {ElapsedMilliseconds} ms", requestName, monitor.ElapsedMilliseconds);

            return response;
        }
        catch (Exception)
        {
            monitor.Stop();
            _logger.LogError("Request {RequestName} falhou após {ElapsedMilliseconds} ms", requestName, monitor.ElapsedMilliseconds);
            throw;
        }
    }
}
