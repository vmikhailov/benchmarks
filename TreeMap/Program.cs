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

        // Demo both implementations
        Console.WriteLine("Comparing Dictionary vs BST implementations:\n");

        DemoDictionaryImplementation();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        DemoBSTImplementation();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        DemoSortedArrayImplementation();
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // Advanced test data demos
        DemoAdvancedTestData();
    }

    static void DemoDictionaryImplementation()
    {
        Console.WriteLine("DICTIONARY IMPLEMENTATION");
        Console.WriteLine("Data Structure: Dictionary<(x,y), string>\n");

        var mapStorage = new MapStorage_Dictionary();

        // Add labels using test data generator
        Console.WriteLine("Adding labels...");
        foreach (var (x, y, label) in TestDataGenerator.GetBasicTestData())
        {
            mapStorage.Add(x, y, label);
        }
        Console.WriteLine($"Total labels: {mapStorage.Count}\n");

        DemoCommonOperations(mapStorage);
    }

    static void DemoBSTImplementation()
    {
        Console.WriteLine("BST IMPLEMENTATION");
        Console.WriteLine("Data Structure: Binary Search Tree with key = x² + y²\n");

        var mapStorage = new MapStorage_BST();

        // Add labels using test data generator
        Console.WriteLine("Adding labels...");
        foreach (var (x, y, label) in TestDataGenerator.GetBasicTestData())
        {
            mapStorage.Add(x, y, label);
        }
        Console.WriteLine($"Total labels: {mapStorage.Count}\n");

        DemoCommonOperations(mapStorage);

        // Show BST-specific statistics
        var stats = mapStorage.GetStatistics();
        Console.WriteLine("\nBST Statistics:");
        Console.WriteLine($"  Tree nodes: {stats.nodes}");
        Console.WriteLine($"  Tree height: {stats.treeHeight}");
        Console.WriteLine($"  Total collisions: {stats.totalCollisions}");
        Console.WriteLine($"  Max collisions per node: {stats.maxCollisions}");

        // Demonstrate collisions
        Console.WriteLine("\nCollision Example (x² + y² = 25):");
        mapStorage.Clear();
        mapStorage.Add(3, 4, "point_3_4");  // 3² + 4² = 25
        mapStorage.Add(4, 3, "point_4_3");  // 4² + 3² = 25 (collision!)
        mapStorage.Add(5, 0, "point_5_0");  // 5² + 0² = 25 (collision!)
        mapStorage.Add(0, 5, "point_0_5");  // 0² + 5² = 25 (collision!)

        Console.WriteLine($"  Added 4 labels, all map to key 25");
        Console.WriteLine($"  BST nodes created: {mapStorage.GetStatistics().nodes} (only 1!)");
        Console.WriteLine($"  All stored in collision list:");
        foreach (var (x, y, label) in mapStorage.ListAll())
        {
            Console.WriteLine($"    ({x}, {y}) → {label}");
        }
    }

    static void DemoSortedArrayImplementation()
    {
        Console.WriteLine("SORTED ARRAY IMPLEMENTATION");
        Console.WriteLine("Data Structure: Sorted Array with key = x² + y²\n");

        var mapStorage = new MapStorage_SortedArray();

        // Add labels using test data generator
        Console.WriteLine("Adding labels...");
        foreach (var (x, y, label) in TestDataGenerator.GetBasicTestData())
        {
            mapStorage.Add(x, y, label);
        }
        Console.WriteLine($"Total labels: {mapStorage.Count}\n");

        DemoCommonOperations(mapStorage);

        // Show SortedArray-specific statistics
        var stats = mapStorage.GetStatistics();
        Console.WriteLine("\nSorted Array Statistics:");
        Console.WriteLine($"  Total entries: {stats.totalEntries}");
        Console.WriteLine($"  Unique keys: {stats.uniqueKeys}");
        Console.WriteLine($"  Max collisions per key: {stats.maxCollisions}");

        // Demonstrate efficient radius query
        Console.WriteLine("\nRadius Query Performance (key advantage of sorted array):");
        mapStorage.Clear();

        // Add some test data at various distances
        mapStorage.Add(10, 10, "close_1");      // distance² = 200
        mapStorage.Add(100, 100, "close_2");    // distance² = 20000
        mapStorage.Add(1000, 1000, "medium");   // distance² = 2000000
        mapStorage.Add(5000, 5000, "far");      // distance² = 50000000

        Console.WriteLine($"  Added 4 labels at various distances");
        Console.WriteLine($"  Searching within radius 2000 (radius² = 4000000):");

        var nearby = mapStorage.GetWithinRadius(2000).ToList();
        Console.WriteLine($"  Found {nearby.Count} labels using binary search:");
        foreach (var (x, y, label) in nearby)
        {
            long distSq = (long)x * x + (long)y * y;
            Console.WriteLine($"    ({x}, {y}) → {label} (distance² = {distSq})");
        }
        Console.WriteLine("  Advantage: Binary search to range, no full scan needed!");
    }

    static void DemoCommonOperations(IMapStorage mapStorage)
    {
        // Retrieve labels
        Console.WriteLine("Retrieving labels:");
        Console.WriteLine($"  (1, 1) → {mapStorage.Get(1, 1)}");
        Console.WriteLine($"  (200, 3400) → {mapStorage.Get(200, 3400)}");
        Console.WriteLine($"  (999999, 999999) → {mapStorage.Get(999999, 999999)}");
        Console.WriteLine($"  (100, 100) → {mapStorage.Get(100, 100) ?? "null (not found)"}\n");

        // List all labels
        Console.WriteLine("All labels:");
        foreach (var (x, y, label) in mapStorage.ListAll())
        {
            Console.WriteLine($"  ({x}, {y}) → {label}");
        }
        Console.WriteLine();

        // Region query
        Console.WriteLine("Labels in region (0, 0) to (1000, 5000):");
        foreach (var (x, y, label) in mapStorage.GetInRegion(0, 0, 1000, 5000))
        {
            Console.WriteLine($"  ({x}, {y}) → {label}");
        }
        Console.WriteLine();

        // Radius query
        Console.WriteLine("Labels within radius 600000 from origin (0,0):");
        var withinRadius = mapStorage.GetWithinRadius(600000).ToList();
        Console.WriteLine($"  Found {withinRadius.Count} labels");
        foreach (var (x, y, label) in withinRadius)
        {
            var distanceSquared = (long)x * x + (long)y * y;
            Console.WriteLine($"  ({x}, {y}) → {label} (distance² = {distanceSquared})");
        }
        Console.WriteLine();

        // Remove a label
        Console.WriteLine("Removing label at (1, 1)...");
        var removed = mapStorage.Remove(1, 1);
        Console.WriteLine($"  Removed: {removed}");
        Console.WriteLine($"  Total labels: {mapStorage.Count}");

        // Demonstrate additional test data generators
        DemoAdvancedTestData();
    }

    static void DemoAdvancedTestData()
    {
        Console.WriteLine("=== Advanced Test Data Generation Demo ===\n");

        // Demo 1: Random labels
        Console.WriteLine("1. Random Labels (100 labels):");
        var mapStorage1 = new MapStorage_Dictionary();
        foreach (var (x, y, label) in TestDataGenerator.GenerateRandomLabels(100, seed: 42))
        {
            mapStorage1.Add(x, y, label);
        }
        Console.WriteLine($"   Generated {mapStorage1.Count} random labels");
        var sample = mapStorage1.ListAll().Take(3);
        foreach (var (x, y, label) in sample)
        {
            Console.WriteLine($"   Sample: ({x}, {y}) → {label}");
        }
        Console.WriteLine();

        // Demo 2: Grid pattern
        Console.WriteLine("2. Grid Pattern (5x5 = 25 labels):");
        var mapStorage2 = new MapStorage_Dictionary();
        foreach (var (x, y, label) in TestDataGenerator.GenerateGridPattern(5))
        {
            mapStorage2.Add(x, y, label);
        }
        Console.WriteLine($"   Generated {mapStorage2.Count} grid labels");
        foreach (var (x, y, label) in mapStorage2.ListAll().Take(3))
        {
            Console.WriteLine($"   Sample: ({x}, {y}) → {label}");
        }
        Console.WriteLine();

        // Demo 3: Edge cases
        Console.WriteLine("3. Edge Cases:");
        var mapStorage3 = new MapStorage_Dictionary();
        foreach (var (x, y, label) in TestDataGenerator.GenerateEdgeCases())
        {
            mapStorage3.Add(x, y, label);
        }
        Console.WriteLine($"   Generated {mapStorage3.Count} edge case labels:");
        foreach (var (x, y, label) in mapStorage3.ListAll())
        {
            Console.WriteLine($"   ({x}, {y}) → {label}");
        }
        Console.WriteLine();

        // Demo 4: Clustered data
        Console.WriteLine("4. Clustered Data (50 labels around center):");
        var mapStorage4 = new MapStorage_Dictionary();
        foreach (var (x, y, label) in TestDataGenerator.GenerateClusteredData(50, 500000, 500000, 10000, seed: 42))
        {
            mapStorage4.Add(x, y, label);
        }
        Console.WriteLine($"   Generated {mapStorage4.Count} clustered labels");
        var clusterSample = mapStorage4.ListAll().Take(3);
        foreach (var (x, y, label) in clusterSample)
        {
            Console.WriteLine($"   Sample: ({x}, {y}) → {label}");
        }

        // Test spatial query on clustered data
        var inRegion = mapStorage4.GetInRegion(490000, 490000, 510000, 510000).Count();
        Console.WriteLine($"   Labels in center region: {inRegion}/{mapStorage4.Count}");
        Console.WriteLine();

        // Demo 5: Mixed dataset
        Console.WriteLine("5. Mixed Dataset (100 labels):");
        var mapStorage5 = new MapStorage_Dictionary();
        foreach (var (x, y, label) in TestDataGenerator.GenerateMixedDataset(100, seed: 42))
        {
            mapStorage5.Add(x, y, label);
        }
        Console.WriteLine($"   Generated {mapStorage5.Count} mixed labels");
        Console.WriteLine($"   Edge cases: {mapStorage5.ListAll().Count(l => l.label.StartsWith("corner") || l.label.StartsWith("edge") || l.label == "center")}");
        Console.WriteLine($"   Grid points: {mapStorage5.ListAll().Count(l => l.label.StartsWith("grid"))}");
        Console.WriteLine($"   Random: {mapStorage5.ListAll().Count(l => l.label.StartsWith("random"))}");
    }
}
