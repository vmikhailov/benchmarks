namespace TreeMap.Tests;

/// <summary>
/// Tests for MapStorage_SortedDictionary implementation.
/// </summary>
public static class SortedDictionaryTest
{
    public static void RunTests()
    {
        Console.WriteLine("=== Sorted Dictionary Implementation Test ===\n");
        Console.WriteLine("Data Structure: SortedDictionary<long, Entry>\n");

        var mapStorage = new MapStorage_SortedDictionary();

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

        // Remove a label
        Console.WriteLine("Removing label at (1, 1)...");
        var removed = mapStorage.Remove(1, 1);
        Console.WriteLine($"  Removed: {removed}");
        Console.WriteLine($"  Total labels: {mapStorage.Count}\n");

        Console.WriteLine("✓ Sorted Dictionary test completed successfully!");
    }
}

/// <summary>
/// Tests for MapStorage_Dictionary implementation.
/// </summary>
public static class DictionaryTest
{
    public static void RunTests()
    {
        Console.WriteLine("=== Dictionary Implementation Test ===\n");
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

        Console.WriteLine("✓ Dictionary test completed successfully!");
    }
}

