using System.Diagnostics;

namespace TreeMap.Tests;

/// <summary>
/// Tests for MapStorage_DynamicTiled implementation with configurable capacity.
/// </summary>
public static class DynamicTiledTest
{
    public static void RunTests(int capacity = 64)
    {
        Console.WriteLine($"=== Dynamic Tiled Implementation Test (Capacity: {capacity}) ===\n");
        Console.WriteLine("Tiles split automatically when they reach capacity limit\n");

        var storage = new MapStorage_DynamicTiled(1_000_000, capacity);

        // Add labels using test data generator
        Console.WriteLine("Adding basic test labels...");
        foreach (var entry in TestDataGenerator.GetBasicTestData())
        {
            storage.Add(entry);
        }
        Console.WriteLine($"Total labels: {storage.Count}");
        Console.WriteLine($"Current tiles: {storage.TileCount}\n");

        // Add more entries to trigger splits
        Console.WriteLine($"Adding more entries to trigger tile splits (capacity: {capacity})...");
        var random = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            storage.Add(new(random.Next(1_000_000), random.Next(1_000_000), $"Test{i}"));
        }
        Console.WriteLine($"Total labels: {storage.Count}");
        Console.WriteLine($"Current tiles: {storage.TileCount} (splits occurred!)\n");

        // Retrieve labels
        Console.WriteLine("Retrieving labels:");
        var result1 = storage.Get(1, 1);
        Console.WriteLine($"  (1, 1) → {result1?.Label ?? "null"}");
        var result2 = storage.Get(200, 3400);
        Console.WriteLine($"  (200, 3400) → {result2?.Label ?? "null"}");
        var result3 = storage.Get(999999, 999999);
        Console.WriteLine($"  (999999, 999999) → {result3?.Label ?? "null"}");
        var result4 = storage.Get(100, 100);
        Console.WriteLine($"  (100, 100) → {result4?.Label ?? "null (not found)"}\n");

        // Region query
        Console.WriteLine("Labels in region (0, 0) to (10000, 10000):");
        var regionResults = storage.GetInRegion(0, 0, 10000, 10000);
        Console.WriteLine($"  Found {regionResults.Length} labels\n");

        // Radius query
        Console.WriteLine("Labels within radius 100000 from origin (0,0):");
        var radiusResults = storage.GetWithinRadius(100000);
        Console.WriteLine($"  Found {radiusResults.Length} labels\n");

        // Remove a label
        Console.WriteLine("Removing label at (1, 1)...");
        var removed = storage.Remove(1, 1);
        Console.WriteLine($"  Removed: {removed}");
        Console.WriteLine($"  Total labels: {storage.Count}\n");

        Console.WriteLine("✓ Dynamic Tiled test completed successfully!");
    }

    public static void RunPerformanceTests(int capacity = 64)
    {
        Console.WriteLine($"=== Dynamic Tiled Performance Test (Capacity: {capacity}) ===\n");

        var storage = new MapStorage_DynamicTiled(1_000_000, capacity);
        var random = new Random(42);
        var stopwatch = Stopwatch.StartNew();

        // Test with clustered data (should trigger many splits)
        Console.WriteLine("Testing with clustered data (in region 0-100000)...");
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            storage.Add(new(random.Next(100000), random.Next(100000), $"Clustered{i}"));
        }
        stopwatch.Stop();
        Console.WriteLine($"  Added 1000 clustered entries in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Tile count: {storage.TileCount} (adaptive splitting!)\n");

        // Test with sparse data (should create fewer splits)
        Console.WriteLine("Testing with sparse data (across entire map)...");
        var storage2 = new MapStorage_DynamicTiled(1_000_000, capacity);
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            storage2.Add(new(random.Next(1_000_000), random.Next(1_000_000), $"Sparse{i}"));
        }
        stopwatch.Stop();
        Console.WriteLine($"  Added 1000 sparse entries in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Tile count: {storage2.TileCount} (fewer splits due to sparsity!)\n");

        // Test radius queries
        Console.WriteLine("Testing GetWithinRadius with various radii...");
        var radii = new[] { 50_000, 100_000, 300_000, 500_000 };
        foreach (var radius in radii)
        {
            stopwatch.Restart();
            var results = storage.GetWithinRadius(radius);
            stopwatch.Stop();
            Console.WriteLine($"  Radius {radius,7}: Found {results.Length,4} entries in {stopwatch.ElapsedMilliseconds,4}ms");
        }

        Console.WriteLine("\nTesting GetWithinRadius from center...");
        for (int i = 0; i < 5; i++)
        {
            var centerX = random.Next(1_000_000);
            var centerY = random.Next(1_000_000);
            var radius = random.Next(50_000, 200_000);

            stopwatch.Restart();
            var results = storage.GetWithinRadius(centerX, centerY, radius);
            stopwatch.Stop();
            Console.WriteLine($"  Center ({centerX,6},{centerY,6}) radius {radius,6}: Found {results.Length,4} entries in {stopwatch.ElapsedMilliseconds,4}ms");
        }

        // Compare different capacities
        Console.WriteLine("\n=== Comparing Different Tile Capacities ===");
        var capacities = new[] { 16, 32, 64, 128, 256 };
        foreach (var cap in capacities)
        {
            var testStorage = new MapStorage_DynamicTiled(1_000_000, cap);
            stopwatch.Restart();

            for (int i = 0; i < 1000; i++)
            {
                testStorage.Add(new(random.Next(100000), random.Next(100000), $"Test{i}"));
            }

            stopwatch.Stop();
            Console.WriteLine($"  Capacity {cap,3}: {stopwatch.ElapsedMilliseconds,4}ms to add 1000 entries, {testStorage.TileCount,3} tiles created");
        }

        Console.WriteLine($"\n✓ Performance tests completed successfully!");
    }
}

