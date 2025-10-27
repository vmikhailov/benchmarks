namespace TreeMap;

/// <summary>
/// Quick test to verify MapStorage_Tiled implementation correctness
/// </summary>
public class TiledStorageTest
{
    public static void RunTests()
    {
        Console.WriteLine("Testing MapStorage_Tiled implementation...\n");

        TestBasicOperations();
        TestGetInRegion();
        TestGetWithinRadius();
        TestGetWithinRadiusFromCenter();

        Console.WriteLine("\n✓ All tests passed!");
    }

    private static void TestBasicOperations()
    {
        Console.WriteLine("Test: Basic operations (Add, Get, Remove, Contains)");
        var storage = new MapStorage_Tiled();

        // Add some entries
        var entry1 = new Entry(100, 200, "Label1");
        var entry2 = new Entry(1500, 2500, "Label2");
        var entry3 = new Entry(500_000, 500_000, "Label3");

        Assert(storage.Add(entry1), "Should add new entry");
        Assert(!storage.Add(entry1), "Should not add duplicate");
        Assert(storage.Add(entry2), "Should add new entry");
        Assert(storage.Add(entry3), "Should add new entry");

        // Test Get
        Assert(storage.Get(100, 200)?.Label == "Label1", "Should get Label1");
        Assert(storage.Get(1500, 2500)?.Label == "Label2", "Should get Label2");
        Assert(storage.Get(999, 999) == null, "Should return null for non-existent");

        // Test Contains
        Assert(storage.Contains(100, 200), "Should contain entry");
        Assert(!storage.Contains(999, 999), "Should not contain non-existent");

        // Test Remove
        Assert(storage.Remove(100, 200), "Should remove entry");
        Assert(!storage.Remove(100, 200), "Should not remove twice");
        Assert(storage.Get(100, 200) == null, "Should not find removed entry");

        // Test Count
        Assert(storage.Count == 2, $"Count should be 2, got {storage.Count}");

        Console.WriteLine("  ✓ Basic operations work correctly\n");
    }

    private static void TestGetInRegion()
    {
        Console.WriteLine("Test: GetInRegion");
        var storage = new MapStorage_Tiled();

        // Add entries in different tiles
        storage.Add(new(1000, 1000, "A"));
        storage.Add(new(2000, 2000, "B"));
        storage.Add(new(5000, 5000, "C"));
        storage.Add(new(10000, 10000, "D"));
        storage.Add(new(100000, 100000, "E"));

        // Test region that includes some entries
        var result = storage.GetInRegion(0, 0, 6000, 6000);
        Assert(result.Length == 3, $"Should find 3 entries, found {result.Length}");
        Assert(result.Any(e => e.Label == "A"), "Should find A");
        Assert(result.Any(e => e.Label == "B"), "Should find B");
        Assert(result.Any(e => e.Label == "C"), "Should find C");

        // Test region with no entries
        result = storage.GetInRegion(20000, 20000, 30000, 30000);
        Assert(result.Length == 0, $"Should find 0 entries, found {result.Length}");

        // Test region that spans multiple tiles
        result = storage.GetInRegion(0, 0, 150000, 150000);
        Assert(result.Length == 5, $"Should find all 5 entries, found {result.Length}");

        Console.WriteLine("  ✓ GetInRegion works correctly\n");
    }

    private static void TestGetWithinRadius()
    {
        Console.WriteLine("Test: GetWithinRadius from origin");
        var storage = new MapStorage_Tiled();

        // Add entries at various distances from origin
        storage.Add(new(100, 100, "Near"));
        storage.Add(new(1000, 1000, "Mid"));
        storage.Add(new(10000, 10000, "Far"));

        // Small radius - should find one
        var result = storage.GetWithinRadius(200);
        Assert(result.Length == 1, $"Small radius should find 1 entry, found {result.Length}");
        Assert(result[0].Label == "Near", "Should find Near");

        // Medium radius - should find two
        result = storage.GetWithinRadius(2000);
        Assert(result.Length == 2, $"Medium radius should find 2 entries, found {result.Length}");

        // Large radius - should find all
        result = storage.GetWithinRadius(20000);
        Assert(result.Length == 3, $"Large radius should find 3 entries, found {result.Length}");

        Console.WriteLine("  ✓ GetWithinRadius from origin works correctly\n");
    }

    private static void TestGetWithinRadiusFromCenter()
    {
        Console.WriteLine("Test: GetWithinRadius from center point");
        var storage = new MapStorage_Tiled();

        // Add entries around a center point (10000, 10000)
        storage.Add(new(10000, 10000, "Center"));
        storage.Add(new(10500, 10500, "Close"));
        storage.Add(new(12000, 12000, "Medium"));
        storage.Add(new(15000, 15000, "Far"));
        storage.Add(new(100, 100, "VeryFar"));

        // Small radius from center
        var result = storage.GetWithinRadius(10000, 10000, 1000);
        Assert(result.Length == 2, $"Should find 2 entries, found {result.Length}");
        Assert(result.Any(e => e.Label == "Center"), "Should find Center");
        Assert(result.Any(e => e.Label == "Close"), "Should find Close");

        // Medium radius from center
        result = storage.GetWithinRadius(10000, 10000, 3000);
        Assert(result.Length == 3, $"Should find 3 entries, found {result.Length}");

        // Large radius from center
        result = storage.GetWithinRadius(10000, 10000, 8000);
        Assert(result.Length == 4, $"Should find 4 entries, found {result.Length}");

        Console.WriteLine("  ✓ GetWithinRadius from center works correctly\n");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new($"Assertion failed: {message}");
        }
    }
}

