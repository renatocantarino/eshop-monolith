using System.Collections.Concurrent;
using System.Diagnostics;

const int totalRequests = 1000;
const int maxParallelism = 50; // Simulando 50 usuários simultâneos
var results = new ConcurrentBag<long>(); // Armazena latência em ms

using var httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://localhost:7029"),
    Timeout = TimeSpan.FromSeconds(30)
};

Console.WriteLine($"Iniciando teste: {totalRequests} requisições, {maxParallelism} simultâneas.");
var swTotal = Stopwatch.StartNew();

await Parallel.ForEachAsync(Enumerable.Range(0, totalRequests),
    new ParallelOptions { MaxDegreeOfParallelism = maxParallelism },
    async (i, token) =>
    {
        var swRequest = Stopwatch.StartNew();
        try
        {
            var response = await httpClient.GetAsync("/products", token);
            swRequest.Stop();
            results.Add(swRequest.ElapsedMilliseconds);
        }
        catch { /* Ignorar falhas para não travar o teste */ }
    });

swTotal.Stop();

// --- Relatório de Performance ---
var orderedResults = results.OrderBy(x => x).ToList();
Console.WriteLine("\n--- Resultados ---");
Console.WriteLine($"Tempo Total: {swTotal.Elapsed.TotalSeconds:F2}s");
Console.WriteLine($"Requisições/seg: {totalRequests / swTotal.Elapsed.TotalSeconds:F2}");
if (orderedResults.Any())
{
    Console.WriteLine($"Latência Média: {orderedResults.Average():F2}ms");
    Console.WriteLine($"P95 (95% das reqs): {orderedResults[(int)(orderedResults.Count * 0.95)]}ms");
}