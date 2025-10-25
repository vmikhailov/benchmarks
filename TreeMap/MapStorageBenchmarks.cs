using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace TreeMap;

public enum StorageType
{
    Dictionary,
    BST,
    SortedArray,
    SortedDictionary
}

/// <summary>
/// Benchmarks comparing different implementations of IMapStorage.
/// Tests various operations with different data sizes.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[HideColumns("Error", "StdDev", "Median")]
public class MapStorageBenchmarks
{
    private IMapStorage _prePopulatedStorage = null!;
    private List<(int x, int y, string label)> _testData = null!;
    private List<(int x, int y)> _lookupCoordinates = null!;

    [Params(StorageType.Dictionary, StorageType.BST, StorageType.SortedArray, StorageType.SortedDictionary)]
    public StorageType Storage { get; set; }

    [Params(1000)]
    public int LabelCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Generate test data
        _testData = TestDataGenerator.GenerateRandomLabels(LabelCount, seed: 42).ToList();

        // Prepare lookup coordinates (mix of existing and non-existing)
        _lookupCoordinates = new();

        // Add existing coordinates
        _lookupCoordinates.AddRange(_testData.Take(500).Select(t => (t.x, t.y)));

        // Add non-existing coordinates
        var random = new Random(123);

        for (var i = 0; i < 20; i++)
        {
            _lookupCoordinates.Add((random.Next(1_000_000), random.Next(1_000_000)));
        }

        // Pre-populate storage
        _prePopulatedStorage = CreateStorage();
        foreach (var (x, y, label) in _testData)
        {
            _prePopulatedStorage.Add(x, y, label);
        }
    }

    private IMapStorage CreateStorage() => Storage switch
    {
        StorageType.Dictionary => new MapStorage_Dictionary(),
        StorageType.BST => new MapStorage_BST(),
        StorageType.SortedArray => new MapStorage_SortedArray(),
        StorageType.SortedDictionary => new MapStorage_SortedDictionary(),
        _ => throw new ArgumentOutOfRangeException()
    };

    [Benchmark]
    [BenchmarkCategory("Add")]
    public void Add()
    {
        var storage = CreateStorage();

        foreach (var (x, y, label) in _testData)
        {
            storage.Add(x, y, label);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Get")]
    public void Get()
    {
        foreach (var (x, y) in _lookupCoordinates)
        {
            _prePopulatedStorage.Get(x, y);
        }
    }

    [Benchmark]
    [BenchmarkCategory("ListAll")]
    public void ListAll()
    {
        var result = _prePopulatedStorage.ListAll().ToList();
    }

    [Benchmark]
    [BenchmarkCategory("GetInRegion")]
    public void GetInRegion()
    {
        var result = _prePopulatedStorage.GetInRegion(400000, 400000, 600000, 600000).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("GetWithinRadius")]
    public void GetWithinRadius()
    {
        for (var i = 0; i < 100; i++)
        {
            var r = Random.Shared.Next(1_000_000);
            var result = _prePopulatedStorage.GetWithinRadius(r).ToList();
        }
    }

    [Benchmark]
    [BenchmarkCategory("AddRemove")]
    public void AddRemove()
    {
        var storage = CreateStorage();

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
/// Benchmarks specifically for collision-heavy scenarios.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[HideColumns("Error", "StdDev")]
public class CollisionBenchmarks
{
    private List<(int x, int y, string label)> _collisionData = null!;

    [Params(StorageType.Dictionary, StorageType.BST, StorageType.SortedArray, StorageType.SortedDictionary)]
    public StorageType Storage { get; set; }

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

    private IMapStorage CreateStorage() => Storage switch
    {
        StorageType.Dictionary => new MapStorage_Dictionary(),
        StorageType.BST => new MapStorage_BST(),
        StorageType.SortedArray => new MapStorage_SortedArray(),
        StorageType.SortedDictionary => new MapStorage_SortedDictionary(),
        _ => throw new ArgumentOutOfRangeException()
    };

    [Benchmark]
    [BenchmarkCategory("Collisions")]
    public void WithCollisions()
    {
        var storage = CreateStorage();

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
