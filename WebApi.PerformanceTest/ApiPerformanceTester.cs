using System.Diagnostics;

namespace WebApi.PerformanceTest;

public class ApiPerformanceTester
{
    private readonly HttpClient _httpClient;

    public ApiPerformanceTester()
    {
        _httpClient = new();
    }

    public async Task<PerformanceResult> TestEndpointAsync(
        string endpointName,
        string fullUrl,
        int numberOfRequests,
        int concurrentRequests = 1)
    {
        var requestTimes = new List<double>();
        var successCount = 0;
        var failCount = 0;

        var totalStopwatch = Stopwatch.StartNew();

        // Create batches of concurrent requests
        var batches = Enumerable.Range(0, numberOfRequests)
            .Select((_, i) => i)
            .GroupBy(x => x / concurrentRequests)
            .Select(g => g.ToList())
            .ToList();

        foreach (var batch in batches)
        {
            var tasks = batch.Select(async _ =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var response = await _httpClient.GetAsync(fullUrl);
                    stopwatch.Stop();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref failCount);
                    }

                    return stopwatch.Elapsed.TotalMilliseconds;
                }
                catch
                {
                    stopwatch.Stop();
                    Interlocked.Increment(ref failCount);
                    return stopwatch.Elapsed.TotalMilliseconds;
                }
            }).ToList();

            var times = await Task.WhenAll(tasks);
            lock (requestTimes)
            {
                requestTimes.AddRange(times);
            }
        }

        totalStopwatch.Stop();

        var sortedTimes = requestTimes.OrderBy(x => x).ToList();

        return new()
        {
            EndpointName = endpointName,
            TotalRequests = numberOfRequests,
            SuccessfulRequests = successCount,
            FailedRequests = failCount,
            TotalTimeMs = totalStopwatch.Elapsed.TotalMilliseconds,
            AverageTimeMs = requestTimes.Average(),
            MinTimeMs = requestTimes.Min(),
            MaxTimeMs = requestTimes.Max(),
            RequestsPerSecond = numberOfRequests / totalStopwatch.Elapsed.TotalSeconds,
            MedianTimeMs = GetPercentile(sortedTimes, 0.5),
            P95TimeMs = GetPercentile(sortedTimes, 0.95),
            P99TimeMs = GetPercentile(sortedTimes, 0.99)
        };
    }

    private static double GetPercentile(List<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return 0;
        
        var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
        index = Math.Max(0, Math.Min(index, sortedValues.Count - 1));
        return sortedValues[index];
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
