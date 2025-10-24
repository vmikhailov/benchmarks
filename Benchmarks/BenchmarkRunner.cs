using System.Diagnostics;
using System.Runtime;

namespace Primes1;

public class BenchmarkRunner
{
    private readonly IBenchmark _benchmark;
    private readonly int _scale;
    private readonly int _iterations;
    private readonly int _warmupScale;

    public BenchmarkRunner(IBenchmark benchmark, int scale, int iterations = 10, int? warmupScale = null)
    {
        _benchmark = benchmark;
        _scale = scale;
        _iterations = iterations;
        _warmupScale = warmupScale ?? scale / 10;
    }

    public BenchmarkResult Run()
    {
        Console.WriteLine($"{_benchmark.GetName()} - Benchmark Mode");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine();

        Console.WriteLine("Configuration:");
        Console.WriteLine($"- Scale ({_benchmark.GetScaleUnit()}): {_scale:N0}");
        Console.WriteLine($"- Test iterations: {_iterations}");
        Console.WriteLine($"- Server GC: {GCSettings.IsServerGC}");
        Console.WriteLine($"- Tiered Compilation: {GetTieredCompilationStatus()}");
        Console.WriteLine();

        // Warmup run
        Console.WriteLine($"Performing warmup run (scale: {_warmupScale:N0})...");
        var warmupWatch = Stopwatch.StartNew();
        var warmupResult = _benchmark.Execute(_warmupScale);
        warmupWatch.Stop();
        Console.WriteLine($"Warmup completed in {warmupWatch.Elapsed.TotalSeconds:F4} seconds");
        Console.WriteLine($"Warmup metric: {_benchmark.GetMetric(warmupResult):N0}");
        Console.WriteLine();

        // Store execution times
        var executionTimes = new List<long>();
        var memoryUsages = new List<long>();
        object? result = null;

        Console.WriteLine($"Running {_iterations} benchmark iterations...");
        Console.WriteLine(new string('-', 70));

        for (int i = 1; i <= _iterations; i++)
        {
            // Memory tracking
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryBefore = GC.GetTotalMemory(false);
            var stopwatch = Stopwatch.StartNew();

            result = _benchmark.Execute(_scale);

            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;

            executionTimes.Add(stopwatch.ElapsedMilliseconds);
            memoryUsages.Add(memoryUsed);

            Console.WriteLine($"Run {i,2}: {stopwatch.ElapsedMilliseconds,7} ms ({stopwatch.Elapsed.TotalSeconds:F3} sec) | Memory: {memoryUsed / 1024.0 / 1024.0,8:F2} MB");
        }

        Console.WriteLine();
        Console.WriteLine("=== STATISTICS ===");
        Console.WriteLine("Execution Time:");
        Console.WriteLine($"  Average:  {executionTimes.Average(),9:F2} ms");
        Console.WriteLine($"  Min:      {executionTimes.Min(),9} ms");
        Console.WriteLine($"  Max:      {executionTimes.Max(),9} ms");
        Console.WriteLine($"  Median:   {GetMedian(executionTimes),9:F2} ms");
        Console.WriteLine($"  StdDev:   {GetStandardDeviation(executionTimes),9:F2} ms");
        Console.WriteLine();
        Console.WriteLine("Memory Usage:");
        Console.WriteLine($"  Average:  {memoryUsages.Average() / 1024.0 / 1024.0,9:F2} MB");
        Console.WriteLine($"  Min:      {memoryUsages.Min() / 1024.0 / 1024.0,9:F2} MB");
        Console.WriteLine($"  Max:      {memoryUsages.Max() / 1024.0 / 1024.0,9:F2} MB");
        Console.WriteLine($"  Median:   {GetMedian(memoryUsages) / 1024.0 / 1024.0,9:F2} MB");
        Console.WriteLine();

        // if (result != null)
        // {
        //     Console.WriteLine("=== SAMPLE OUTPUT ===");
        //     Console.WriteLine(_benchmark.GetSample(result));
        //     Console.WriteLine();
        // }

        return new()
        {
            BenchmarkName = _benchmark.GetName(),
            AvgTime = executionTimes.Average() / 1000.0,
            MinTime = executionTimes.Min() / 1000.0,
            MaxTime = executionTimes.Max() / 1000.0,
            MedianTime = GetMedian(executionTimes) / 1000.0,
            StdDev = GetStandardDeviation(executionTimes) / 1000.0,
            AvgMemory = memoryUsages.Average() / 1024.0 / 1024.0,
            MinMemory = memoryUsages.Min() / 1024.0 / 1024.0,
            MaxMemory = memoryUsages.Max() / 1024.0 / 1024.0,
            Metric = result != null ? _benchmark.GetMetric(result) : 0
        };
    }

    private static double GetMedian(List<long> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private static double GetStandardDeviation(List<long> values)
    {
        double avg = values.Average();
        double sumOfSquares = values.Sum(val => (val - avg) * (val - avg));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    private static string GetTieredCompilationStatus()
    {
        // Check environment variable and AppContext
        var envVar = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") 
                     ?? Environment.GetEnvironmentVariable("COMPlus_TieredCompilation");
        
        // In .NET 6+, tiered compilation is enabled by default unless explicitly disabled
        if (envVar == "0")
            return "Disabled (via env var)";
        
        // Try to check via AppContext (this is the most reliable way)
        if (AppContext.TryGetSwitch("System.Runtime.TieredCompilation", out bool isEnabled))
            return isEnabled ? "Enabled" : "Disabled";
        
        // Default for .NET 6+ is enabled
        return "Enabled (default)";
    }
}
