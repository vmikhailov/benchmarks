using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace TreeMap;

/// <summary>
/// Benchmarks comparing different implementations of IMapStorage.
/// Tests various operations with different data sizes.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[HideColumns("Error", "StdDev", "Median")]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class StandardBenchmarks
{
    private IMapStorage _prePopulatedStorage = null!;
    private List<Entry> _testData = null!;
    private List<(int x, int y)> _lookupCoordinates = null!;

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    public static IMapStorageFactory[] StorageTypes =>
    [
        new StorageFactory<MapStorage_BST>("BST"),
        new StorageFactory<MapStorage_Dictionary>("DictLongKey"),
        new StorageFactory<MapStorage_SortedArray>("SortedArray"),
        //new StorageFactory<MapStorage_SortedDictionary>("SortedDict"),
        new StorageFactory<MapStorage_StringKey>("DictStringKey"),
        new StorageFactory<MapStorage_Tiled>("Tiled"),
        new StorageFactory<MapStorage_DynamicTiled>("Dymamic", 1_000_000, 256),
    ];

    [Params(1000, 10000, 50000)]
    public int LabelCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Generate test data
        _testData = TestDataGenerator.GenerateRandomLabels(LabelCount, seed: 42).ToList();

        // Prepare lookup coordinates (mix of existing and non-existing)
        _lookupCoordinates = [];

        // Add existing coordinates
        _lookupCoordinates.AddRange(_testData.Take(20).Select(t => (t.X, t.Y)));

        // Add non-existing coordinates
        var random = new Random(123);

        for (var i = 0; i < 20; i++)
        {
            _lookupCoordinates.Add((random.Next(1_000_000), random.Next(1_000_000)));
        }

        // Pre-populate storage
        _prePopulatedStorage = CreateStorage();

        foreach (var entry in _testData)
        {
            _prePopulatedStorage.Add(entry);
        }
    }

    private IMapStorage CreateStorage() => Storage.Create();

    [Benchmark]
    [BenchmarkCategory("Add")]
    public void Add()
    {
        var storage = CreateStorage();

        foreach (var entry in _testData)
        {
            storage.Add(entry);
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
    [BenchmarkCategory("GetWithinRadiusFromCenter")]
    public void GetWithinRadiusFromCenter()
    {
        for (var i = 0; i < 100; i++)
        {
            var centerX = Random.Shared.Next(1_000_000);
            var centerY = Random.Shared.Next(1_000_000);
            var r = Random.Shared.Next(100_000, 500_000);
            var result = _prePopulatedStorage.GetWithinRadius(centerX, centerY, r).ToList();
        }
    }

    [Benchmark]
    [BenchmarkCategory("GetWithinSmallRadiusFromCenter")]
    public void GetWithinSmallRadiusFromCenter()
    {
        for (var i = 0; i < 100; i++)
        {
            var centerX = Random.Shared.Next(1_000_000);
            var centerY = Random.Shared.Next(1_000_000);
            var r = Random.Shared.Next(1000, 100_000);
            var result = _prePopulatedStorage.GetWithinRadius(centerX, centerY, r).ToList();
        }
    }

    [Benchmark]
    [BenchmarkCategory("AddRemove")]
    public void AddRemove()
    {
        var storage = CreateStorage();

        // Add items
        foreach (var entry in _testData.Take(50))
        {
            storage.Add(entry);
        }

        // Remove items
        foreach (var entry in _testData.Take(25))
        {
            storage.Remove(entry.X, entry.Y);
        }
    }
}
