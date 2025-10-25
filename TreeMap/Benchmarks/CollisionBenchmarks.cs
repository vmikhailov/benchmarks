using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace TreeMap;

/// <summary>
/// Benchmarks specifically for collision-heavy scenarios.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[HideColumns("Error", "StdDev")]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class CollisionBenchmarks
{
    private List<Entry> _collisionData = null!;

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    public static IMapStorageFactory[] StorageTypes =>
    [
        new StorageFactory<MapStorage_Dictionary>("Dictionary"),
        new StorageFactory<MapStorage_BST>("BST"),
        new StorageFactory<MapStorage_SortedArray>("SortedArray"),
        new StorageFactory<MapStorage_SortedDictionary>("SortedDict"),
        new StorageFactory<MapStorage_SortedDictionary2>("SortedDict2")
    ];

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
                    _collisionData.Add(new Entry(x, y, $"label_{counter++}"));
                    _collisionData.Add(new Entry(y, x, $"label_{counter++}")); // Swap for collision
                }
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("Collisions")]
    public void WithCollisions()
    {
        var storage = Storage.Create();

        foreach (var entry in _collisionData)
        {
            storage.Add(entry);
        }

        // Retrieve all
        foreach (var entry in _collisionData)
        {
            storage.Get(entry.X, entry.Y);
        }
    }
}