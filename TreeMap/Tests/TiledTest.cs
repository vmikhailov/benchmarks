using System.Diagnostics;

namespace TreeMap.Tests;

/// <summary>
/// Tests for MapStorage_Tiled implementation with configurable tile size.
/// </summary>
public static class TiledTest
{
    public static void RunTests(int tileSize = 16)
    {
        Console.WriteLine($"=== Tiled Implementation Test (Tile Size: {tileSize}) ===\n");

        var storage = new MapStorage_Tiled(1_000_000, tileSize);

        // Add some test entries
        Console.WriteLine("Adding test entries...");
        storage.Add(new(10000, 10000, "Test1"));
        storage.Add(new(500000, 500000, "Test2"));
        storage.Add(new(100, 100, "Test3"));
        Console.WriteLine($"Added {storage.Count} entries\n");

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

        Console.WriteLine("\n✓ Basic tests completed successfully!\n");
    }

    public static void RunPerformanceTests(int tileSize = 16)
    {
        Console.WriteLine($"=== Tiled Performance Test (Tile Size: {tileSize}) ===\n");

        var storage = new MapStorage_Tiled(1_000_000, tileSize);
        var random = new Random(42);

        // Add 1000 entries
        Console.WriteLine("Adding 1000 entries...");
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            storage.Add(new(random.Next(1_000_000), random.Next(1_000_000), $"Label{i}"));
        }
        stopwatch.Stop();
        Console.WriteLine($"  Completed in {stopwatch.ElapsedMilliseconds}ms\n");

        // Test with large radius queries (the ones that were causing hangs)
        Console.WriteLine("Testing GetWithinRadius with large radii (these were causing hangs)...");
        var radii = new[] { 100_000, 300_000, 500_000, 800_000 };
        foreach (var radius in radii)
        {
            stopwatch.Restart();
            var results = storage.GetWithinRadius(radius);
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
            var results = storage.GetWithinRadius(centerX, centerY, radius);
            stopwatch.Stop();

            if (i < 3) // Only print first few
                Console.WriteLine($"  Center ({centerX,6},{centerY,6}) radius {radius,6}: Found {results.Length,4} entries in {stopwatch.ElapsedMilliseconds,4}ms");
        }

        Console.WriteLine($"\n✓ Performance tests completed successfully!");
        Console.WriteLine($"Storage contains {storage.Count} entries\n");
    }
}

