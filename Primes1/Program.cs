using Primes1;

Console.WriteLine("=======================================================================");
Console.WriteLine("                    PERFORMANCE BENCHMARK SUITE");
Console.WriteLine("=======================================================================");
Console.WriteLine();

// Parse command line arguments
var benchmarkType = args.Length > 0 ? args[0] : "all";
var iterations = args.Length > 1 && int.TryParse(args[1], out var iter) ? iter : 20;

var results = new List<BenchmarkResult>();

// Define benchmarks
var benchmarks = new Dictionary<string, (IBenchmark Instance, int Scale, int WarmupScale)>
{
    ["primes"] = (new PrimeBenchmark(), 10_000_000, 1_000_000),
    ["primes2"] = (new PrimeBenchmark2(), 10_000_000, 1_000_000),
    ["json"] = (new JsonBenchmark(), 100_000, 10_000),
    ["regex"] = (new RegexBenchmark(), 50_000, 5_000)
};

// Determine which benchmarks to run
List<string> toRun;
if (benchmarkType == "all")
{
    toRun = benchmarks.Keys.ToList();
}
else if (benchmarks.ContainsKey(benchmarkType))
{
    toRun = [benchmarkType];
}
else
{
    Console.WriteLine($"Error: Unknown benchmark type '{benchmarkType}'");
    Console.WriteLine($"Available benchmarks: {string.Join(", ", benchmarks.Keys)}, all");
    Console.WriteLine("Usage: dotnet run [benchmark_type] [iterations]");
    Console.WriteLine("Example: dotnet run primes 10");
    Console.WriteLine("Example: dotnet run all 5");
    return 1;
}

// Run selected benchmarks
for (int i = 0; i < toRun.Count; i++)
{
    var name = toRun[i];
    var config = benchmarks[name];
    var runner = new BenchmarkRunner(
        config.Instance,
        config.Scale,
        iterations,
        config.WarmupScale
    );

    var result = runner.Run();
    results.Add(result);

    // Add spacing between benchmarks
    if (toRun.Count > 1 && i < toRun.Count - 1)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine(new string('=', 70));
        Console.WriteLine();
    }
}

// Summary if running multiple benchmarks
if (results.Count > 1)
{
    Console.WriteLine();
    Console.WriteLine("=======================================================================");
    Console.WriteLine("                         SUMMARY COMPARISON");
    Console.WriteLine("=======================================================================");
    Console.WriteLine();

    Console.WriteLine($"{"Benchmark",-35} {"Avg Time (s)",12} {"Min Time (s)",12} {"Std Dev (s)",12}");
    Console.WriteLine(new string('-', 70));

    foreach (var result in results)
    {
        Console.WriteLine($"{result.BenchmarkName,-35} {result.AvgTime,12:F4} {result.MinTime,12:F4} {result.StdDev,12:F4}");
    }

    Console.WriteLine();
    Console.WriteLine($"{"Benchmark",-35} {"Avg Mem (MB)",12} {"Peak Mem (MB)",13}");
    Console.WriteLine(new string('-', 70));

    foreach (var result in results)
    {
        Console.WriteLine($"{result.BenchmarkName,-35} {result.AvgMemory,12:F2} {result.MaxMemory,13:F2}");
    }
}

return 0;
