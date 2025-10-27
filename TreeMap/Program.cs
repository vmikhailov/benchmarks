using System.Diagnostics;
using BenchmarkDotNet.Running;
using TreeMap;

// Check if user wants to run tests or benchmarks
if (args.Length > 0)
{
    if (args[0].Equals("test-tiled", StringComparison.OrdinalIgnoreCase))
    {
        TiledStorageTest.RunTests();
        return;
    }

    if (args[0].Equals("benchmark", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Running BenchmarkDotNet...\n");

        // Check if specific benchmark class is requested
        if (args.Length > 1)
        {
            switch (args[1].ToLower())
            {
                case "weighted":
                    BenchmarkRunner.Run<WeightedBenchmark>();
                    break;
                case "collision":
                    BenchmarkRunner.Run<CollisionBenchmarks>();
                    break;
                case "standard":
                    BenchmarkRunner.Run<StandardBenchmarks>();
                    break;
                default:
                    Console.WriteLine("Unknown benchmark type. Use: weighted, collision, or standard");
                    break;
            }
        }
        else
        {
            // Run all benchmarks by default
            BenchmarkRunner.Run<WeightedBenchmark>();
        }

        return;
    }
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
var storage = new MapStorage_Tiled();

// Add some test entries
storage.Add(new Entry(10000, 10000, "Test1"));
storage.Add(new Entry(500000, 500000, "Test2"));
storage.Add(new Entry(100, 100, "Test3"));

Console.WriteLine($"Added {storage.Count} entries");

// Test with large radius (this was causing the hang)
Console.WriteLine("Testing with large radius (800000)...");
var startTime = DateTime.Now;
var results = storage.GetWithinRadius(800000);
var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
Console.WriteLine($"Found {results.Length} entries in {elapsed:F2}ms");

// Test with medium radius
Console.WriteLine("\nTesting with medium radius (100000)...");
startTime = DateTime.Now;
results = storage.GetWithinRadius(100000);
elapsed = (DateTime.Now - startTime).TotalMilliseconds;
Console.WriteLine($"Found {results.Length} entries in {elapsed:F2}ms");

// Test GetWithinRadius from center
Console.WriteLine("\nTesting GetWithinRadius from center (500000, 500000, 200000)...");
startTime = DateTime.Now;
results = storage.GetWithinRadius(500000, 500000, 200000);
elapsed = (DateTime.Now - startTime).TotalMilliseconds;
Console.WriteLine($"Found {results.Length} entries in {elapsed:F2}ms");

Console.WriteLine("\n✓ All tests completed successfully!");
Console.WriteLine("Testing MapStorage_Tiled performance with realistic workload...\n");

storage = new MapStorage_Tiled();
var random = new Random(42);

// Add 1000 entries
Console.WriteLine("Adding 1000 entries...");
var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < 1000; i++)
{
    storage.Add(new Entry(random.Next(1_000_000), random.Next(1_000_000), $"Label{i}"));
}
stopwatch.Stop();
Console.WriteLine($"  Completed in {stopwatch.ElapsedMilliseconds}ms\n");

// Test with large radius queries (the ones that were causing hangs)
Console.WriteLine("Testing GetWithinRadius with large radii (these were causing hangs)...");
var radii = new[] { 100_000, 300_000, 500_000, 800_000 };
foreach (var radius in radii)
{
    stopwatch.Restart();
    results = storage.GetWithinRadius(radius);
    stopwatch.Stop();
    Console.WriteLine($"  Radius {radius,7}: Found {results.Length,4} entries in {stopwatch.ElapsedMilliseconds,4}ms");
}

Console.WriteLine("\nTesting GetWithinRadius from center with various radii...");
for (int i = 0; i < 10; i++)
{
    var centerX = random.Next(1_000_000);
    var centerY = random.Next(1_000_000);
    var radius = random.Next(50_000, 300_000);

    stopwatch.Restart();
    results = storage.GetWithinRadius(centerX, centerY, radius);
    stopwatch.Stop();

    if (i < 3) // Only print first few
        Console.WriteLine($"  Center ({centerX,6},{centerY,6}) radius {radius,6}: Found {results.Length,4} entries in {stopwatch.ElapsedMilliseconds,4}ms");
}

Console.WriteLine($"\n✓ All tests completed successfully!");
Console.WriteLine($"Storage contains {storage.Count} entries across {storage.Count} positions");
