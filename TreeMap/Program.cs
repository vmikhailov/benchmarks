using BenchmarkDotNet.Running;

namespace TreeMap;

class Program
{
    static void Main(string[] args)
    {
        // Check if user wants to run benchmarks
        if (args.Length > 0 && args[0].Equals("benchmark", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Running BenchmarkDotNet...\n");
            BenchmarkRunner.Run<MapStorageBenchmarks>();
            //BenchmarkRunner.Run<CollisionBenchmarks>();
            return;
        }

        Console.WriteLine("=== 2D Map Label Storage Demo ===\n");
        Console.WriteLine("To run benchmarks, use: dotnet run --configuration Release -- benchmark\n");

        // Demo Dictionary implementation
        Console.WriteLine("DICTIONARY IMPLEMENTATION");
        Console.WriteLine("Data Structure: Dictionary<(x,y), Entry>\n");

        var mapStorage = new MapStorage_Dictionary();

        // Add labels using test data generator
        Console.WriteLine("Adding labels...");
        foreach (var entry in TestDataGenerator.GetBasicTestData())
        {
            mapStorage.Add(entry);
        }
        Console.WriteLine($"Total labels: {mapStorage.Count}\n");

        // Retrieve labels
        Console.WriteLine("Retrieving labels:");
        var result1 = mapStorage.Get(1, 1);
        Console.WriteLine($"  (1, 1) → {result1?.Label ?? "null"}");
        var result2 = mapStorage.Get(200, 3400);
        Console.WriteLine($"  (200, 3400) → {result2?.Label ?? "null"}");
        var result3 = mapStorage.Get(999999, 999999);
        Console.WriteLine($"  (999999, 999999) → {result3?.Label ?? "null"}");
        var result4 = mapStorage.Get(100, 100);
        Console.WriteLine($"  (100, 100) → {result4?.Label ?? "null (not found)"}\n");

        // List all labels
        Console.WriteLine("All labels:");
        foreach (var entry in mapStorage.ListAll())
        {
            Console.WriteLine($"  ({entry.X}, {entry.Y}) → {entry.Label}");
        }
        Console.WriteLine();

        // Region query
        Console.WriteLine("Labels in region (0, 0) to (1000, 5000):");
        var regionResults = mapStorage.GetInRegion(0, 0, 1000, 5000);
        foreach (var entry in regionResults)
        {
            Console.WriteLine($"  ({entry.X}, {entry.Y}) → {entry.Label}");
        }
        Console.WriteLine();

        // Radius query
        Console.WriteLine("Labels within radius 600000 from origin (0,0):");
        var radiusResults = mapStorage.GetWithinRadius(600000);
        Console.WriteLine($"  Found {radiusResults.Length} labels");
        foreach (var entry in radiusResults)
        {
            var distanceSquared = (long)entry.X * entry.X + (long)entry.Y * entry.Y;
            Console.WriteLine($"  ({entry.X}, {entry.Y}) → {entry.Label} (distance² = {distanceSquared})");
        }
        Console.WriteLine();

        // Remove a label
        Console.WriteLine("Removing label at (1, 1)...");
        var removed = mapStorage.Remove(1, 1);
        Console.WriteLine($"  Removed: {removed}");
        Console.WriteLine($"  Total labels: {mapStorage.Count}\n");

        // Memory efficiency demonstration
        Console.WriteLine("Memory Efficiency:");
        Console.WriteLine($"  Map size: 1,000,000 x 1,000,000 = 1 trillion positions");
        Console.WriteLine($"  Labels stored: {mapStorage.Count}");
        Console.WriteLine($"  Entry records: No tuple allocations on queries!");
        Console.WriteLine($"  Arrays returned: Efficient bulk operations\n");

        Console.WriteLine("=== Demo Complete ===");
        Console.WriteLine("All 4 implementations (Dictionary, BST, SortedArray, SortedDictionary)");
        Console.WriteLine("are available. Run benchmarks to compare performance!");
    }
}

