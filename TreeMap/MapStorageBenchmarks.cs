using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace TreeMap;

/// <summary>
/// Benchmarks comparing Dictionary and BST implementations of IMapStorage.
/// Tests various operations with different data sizes.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MapStorageBenchmarks
{
    private IMapStorage _dictionaryStorage = null!;
    private IMapStorage _bstStorage = null!;
    private IMapStorage _sortedArrayStorage = null!;
    private List<(int x, int y, string label)> _testData = null!;
    private List<(int x, int y)> _lookupCoordinates = null!;

    //[Params(100, 500, 1000)]
    [Params(100, 1000)]
    public int LabelCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Generate test data
        _testData = TestDataGenerator.GenerateRandomLabels(LabelCount, seed: 42).ToList();

        // Prepare lookup coordinates (mix of existing and non-existing)
        _lookupCoordinates = new();

        // Add existing coordinates
        _lookupCoordinates.AddRange(_testData.Take(20).Select(t => (t.x, t.y)));

        // Add non-existing coordinates
        var random = new Random(123);

        for (var i = 0; i < 20; i++)
        {
            _lookupCoordinates.Add((random.Next(1_000_000), random.Next(1_000_000)));
        }

        // Pre-populate storages
        _dictionaryStorage = new MapStorage_Dictionary();
        _bstStorage = new MapStorage_BST();
        _sortedArrayStorage = new MapStorage_SortedArray();

        foreach (var (x, y, label) in _testData)
        {
            _dictionaryStorage.Add(x, y, label);
            _bstStorage.Add(x, y, label);
            _sortedArrayStorage.Add(x, y, label);
        }
    }

    [Benchmark(Description = "Dictionary: Add")]
    [BenchmarkCategory("Add")]
    public void Dictionary_Add()
    {
        var storage = new MapStorage_Dictionary();

        foreach (var (x, y, label) in _testData)
        {
            storage.Add(x, y, label);
        }
    }

    [Benchmark(Description = "BST: Add")]
    [BenchmarkCategory("Add")]
    public void BST_Add()
    {
        var storage = new MapStorage_BST();

        foreach (var (x, y, label) in _testData)
        {
            storage.Add(x, y, label);
        }
    }

    [Benchmark(Description = "SortedArray: Add")]
    [BenchmarkCategory("Add")]
    public void SortedArray_Add()
    {
        var storage = new MapStorage_SortedArray();

        foreach (var (x, y, label) in _testData)
        {
            storage.Add(x, y, label);
        }
    }

    [Benchmark(Description = "Dictionary: Get")]
    [BenchmarkCategory("Get")]
    public void Dictionary_Get()
    {
        foreach (var (x, y) in _lookupCoordinates)
        {
            _dictionaryStorage.Get(x, y);
        }
    }

    [Benchmark(Description = "BST: Get")]
    [BenchmarkCategory("Get")]
    public void BST_Get()
    {
        foreach (var (x, y) in _lookupCoordinates)
        {
            _bstStorage.Get(x, y);
        }
    }

    [Benchmark(Description = "SortedArray: Get")]
    [BenchmarkCategory("Get")]
    public void SortedArray_Get()
    {
        foreach (var (x, y) in _lookupCoordinates)
        {
            _sortedArrayStorage.Get(x, y);
        }
    }

    [Benchmark(Description = "Dictionary: ListAll")]
    [BenchmarkCategory("ListAll")]
    public void Dictionary_ListAll()
    {
        var result = _dictionaryStorage.ListAll().ToList();
    }

    [Benchmark(Description = "BST: ListAll")]
    [BenchmarkCategory("ListAll")]
    public void BST_ListAll()
    {
        var result = _bstStorage.ListAll().ToList();
    }

    [Benchmark(Description = "SortedArray: ListAll")]
    [BenchmarkCategory("ListAll")]
    public void SortedArray_ListAll()
    {
        var result = _sortedArrayStorage.ListAll().ToList();
    }

    [Benchmark(Description = "Dictionary: GetInRegion")]
    [BenchmarkCategory("GetInRegion")]
    public void Dictionary_GetInRegion()
    {
        var result = _dictionaryStorage.GetInRegion(400000, 400000, 600000, 600000).ToList();
    }

    [Benchmark(Description = "BST: GetInRegion")]
    [BenchmarkCategory("GetInRegion")]
    public void BST_GetInRegion()
    {
        var result = _bstStorage.GetInRegion(400000, 400000, 600000, 600000).ToList();
    }

    [Benchmark(Description = "SortedArray: GetInRegion")]
    [BenchmarkCategory("GetInRegion")]
    public void SortedArray_GetInRegion()
    {
        var result = _sortedArrayStorage.GetInRegion(400000, 400000, 600000, 600000).ToList();
    }

    [Benchmark(Description = "Dictionary: GetWithinRadius")]
    [BenchmarkCategory("GetWithinRadius")]
    public void Dictionary_GetWithinRadius()
    {
        var result = _dictionaryStorage.GetWithinRadius(500000).ToList();
    }

    [Benchmark(Description = "BST: GetWithinRadius")]
    [BenchmarkCategory("GetWithinRadius")]
    public void BST_GetWithinRadius()
    {
        var result = _bstStorage.GetWithinRadius(500000).ToList();
    }

    [Benchmark(Description = "SortedArray: GetWithinRadius")]
    [BenchmarkCategory("GetWithinRadius")]
    public void SortedArray_GetWithinRadius()
    {
        var result = _sortedArrayStorage.GetWithinRadius(500000).ToList();
    }

    [Benchmark(Description = "Dictionary: Add/Remove")]
    [BenchmarkCategory("AddRemove")]
    public void Dictionary_AddRemove()
    {
        var storage = new MapStorage_Dictionary();

        // Add items
        foreach (var (x, y, label) in _testData.Take(50))
        {
            storage.Add(x, y, label);
        }

        // Remove items
        foreach (var (x, y, _) in _testData.Take(25))
        {
            storage.Remove(x, y);
        }
    }

    [Benchmark(Description = "BST: Add/Remove")]
    [BenchmarkCategory("AddRemove")]
    public void BST_AddRemove()
    {
        var storage = new MapStorage_BST();

        // Add items
        foreach (var (x, y, label) in _testData.Take(50))
        {
            storage.Add(x, y, label);
        }

        // Remove items
        foreach (var (x, y, _) in _testData.Take(25))
        {
            storage.Remove(x, y);
        }
    }

    [Benchmark(Description = "SortedArray: Add/Remove")]
    [BenchmarkCategory("AddRemove")]
    public void SortedArray_AddRemove()
    {
        var storage = new MapStorage_SortedArray();

        // Add items
        foreach (var (x, y, label) in _testData.Take(50))
        {
            storage.Add(x, y, label);
        }

        // Remove items
        foreach (var (x, y, _) in _testData.Take(25))
        {
            storage.Remove(x, y);
        }
    }
}

/// <summary>
/// Benchmarks specifically for collision-heavy scenarios in BST.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class CollisionBenchmarks
{
    private List<(int x, int y, string label)> _collisionData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Generate data that causes collisions in BST (x² + y² = same value)
        _collisionData = new();

        // Generate Pythagorean triples and their multiples
        var triples = new[]
        {
            (3, 4, 5), // 3² + 4² = 25
            (5, 12, 13), // 5² + 12² = 169
            (8, 15, 17), // 8² + 15² = 289
            (7, 24, 25), // 7² + 24² = 625
        };

        var counter = 0;

        foreach (var (a, b, _) in triples)
        {
            for (var multiplier = 1; multiplier <= 10; multiplier++)
            {
                var x = a * multiplier;
                var y = b * multiplier;

                if (x < 1_000_000 && y < 1_000_000)
                {
                    _collisionData.Add((x, y, $"label_{counter++}"));
                    _collisionData.Add((y, x, $"label_{counter++}")); // Swap for collision
                }
            }
        }
    }

    [Benchmark(Description = "Dictionary: Collisions")]
    [BenchmarkCategory("Collisions")]
    public void Dictionary_WithCollisions()
    {
        var storage = new MapStorage_Dictionary();

        foreach (var (x, y, label) in _collisionData)
        {
            storage.Add(x, y, label);
        }

        // Retrieve all
        foreach (var (x, y, _) in _collisionData)
        {
            storage.Get(x, y);
        }
    }

    [Benchmark(Description = "BST: Collisions")]
    [BenchmarkCategory("Collisions")]
    public void BST_WithCollisions()
    {
        var storage = new MapStorage_BST();

        foreach (var (x, y, label) in _collisionData)
        {
            storage.Add(x, y, label);
        }

        // Retrieve all
        foreach (var (x, y, _) in _collisionData)
        {
            storage.Get(x, y);
        }
    }

    [Benchmark(Description = "SortedArray: Collisions")]
    [BenchmarkCategory("Collisions")]
    public void SortedArray_WithCollisions()
    {
        var storage = new MapStorage_SortedArray();

        foreach (var (x, y, label) in _collisionData)
        {
            storage.Add(x, y, label);
        }

        // Retrieve all
        foreach (var (x, y, _) in _collisionData)
        {
            storage.Get(x, y);
        }
    }
}
