using BenchmarkDotNet.Running;
using TreeMap;
using TreeMap.Benchmarks;
using TreeMap.Tests;

// Check if user wants to run tests or benchmarks
if (args.Length > 0)
{
    var command = args[0].ToLower();

    switch (command)
    {
        case "test":
            TestRunner.Run(args);
            return;

        case "benchmark":
        case "bench":
            RunBenchmark(args);
            return;

        case "help":
        case "--help":
        case "-h":
            PrintUsage();
            return;

        default:
            Console.WriteLine($"Unknown command: {command}");
            Console.WriteLine();
            PrintUsage();
            return;
    }
}

// No arguments - show quick demo
PrintUsage();
Console.WriteLine();
Console.WriteLine("Running quick demo with Dictionary implementation...\n");
DictionaryTest.RunTests();

static void RunBenchmark(string[] args)
{
    Console.WriteLine("Running BenchmarkDotNet...\n");

    if (args.Length > 1)
    {
        switch (args[1].ToLower())
        {
            case "weighted":
                BenchmarkRunner.Run<WeightedBenchmark>();
                break;
            case "weighted_aggregated":
            case "aggregated":
                BenchmarkRunner.Run<WeightedBenchmarkWithCustomAggregation>();
                break;
            case "collision":
                BenchmarkRunner.Run<CollisionBenchmarks>();
                break;
            case "standard":
                BenchmarkRunner.Run<StandardBenchmarks>();
                break;
            default:
                Console.WriteLine($"Unknown benchmark type: {args[1]}");
                Console.WriteLine("Available: weighted, aggregated, collision, standard");
                break;
        }
    }
    else
    {
        // Run default benchmark
        BenchmarkRunner.Run<WeightedBenchmark>();
    }
}

static void PrintUsage()
{
    Console.WriteLine("=== TreeMap - 2D Map Label Storage ===");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  test <impl> [options]  - Run tests for a specific implementation");
    Console.WriteLine("  benchmark [type]       - Run performance benchmarks");
    Console.WriteLine("  help                   - Show this help message");
    Console.WriteLine();
    Console.WriteLine("Test implementations:");
    Console.WriteLine("  dictionary, dict       - Dictionary<(x,y), Entry>");
    Console.WriteLine("  bst                    - Binary Search Tree");
    Console.WriteLine("  sortedarray, array     - Sorted Array");
    Console.WriteLine("  sorteddict             - SortedDictionary");
    Console.WriteLine("  tiled [size] [perf]    - Tiled storage (optional: tile size, perf)");
    Console.WriteLine("  all                    - Run all tests");
    Console.WriteLine();
    Console.WriteLine("Benchmark types:");
    Console.WriteLine("  weighted               - Realistic workload benchmark (default)");
    Console.WriteLine("  aggregated             - With custom aggregation columns");
    Console.WriteLine("  collision              - Collision-heavy scenarios");
    Console.WriteLine("  standard               - Standard benchmarks");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run -- test dictionary");
    Console.WriteLine("  dotnet run -- test tiled 16");
    Console.WriteLine("  dotnet run -- test tiled 16 perf");
    Console.WriteLine("  dotnet run -- test all");
    Console.WriteLine("  dotnet run --configuration Release -- benchmark weighted");
    Console.WriteLine("  dotnet run --configuration Release -- benchmark aggregated");
}

