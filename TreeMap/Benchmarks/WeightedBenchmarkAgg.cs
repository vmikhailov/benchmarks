using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace TreeMap;

/// <summary>
/// Weighted benchmark that simulates realistic usage patterns.
/// Operations: 1000 adds, 100k gets, 1000 ListAll, 10k GetInRegion, 10k GetWithinRadius
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Error", "StdDev", "Median")]
//[SimpleJob(warmupCount: 1, iterationCount: 5)]
public class WeightedBenchmarkAgg
{
    private readonly Dictionary<int, BenchmarkSeedData> _seedDataByNumberOfLabels = new();

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    public static IMapStorageFactory[] StorageTypes =>
    [
        new StorageFactory<MapStorage_BST>("BST"),
        new StorageFactory<MapStorage_Dictionary>("DictLongKey"),
        new StorageFactory<MapStorage_SortedArray>("SortedArray"),
        //new StorageFactory<MapStorage_Tiled>("Tiled_15", 1_000_000, 15),
        new StorageFactory<MapStorage_Tiled>("Tiled_16", 1_000_000, 16),
        //new StorageFactory<MapStorage_Tiled>("Tiled_17", 1_000_000, 17),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_128", 1_000_000, 128),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_256", 1_000_000, 256),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_512", 1_000_000, 512),
//      new StorageFactory<MapStorage_SortedDictionary>("SortedDict"),
//      new StorageFactory<MapStorage_StringKey>("DictStringKey"),
    ];

    private IEnumerable<int> NumberOfLabelsSource()
    {
        var n = 1024;
        var max = Storage.Name == "BST" ? 16384 : 65536;
        while(n <= max)
        {
            yield return n;
            n *= 2;
        }
    }
    
    [GlobalSetup]
    public void Setup()
    {
        foreach (var n in NumberOfLabelsSource())
        {
            var lables = TestDataGenerator.GenerateRandomLabels(n, seed: 42).ToList();
            var seedData = TestDataGenerator.GenerateBenchmarkSeedData(lables, seed: 42);
            _seedDataByNumberOfLabels[n] = seedData;
        }
    }
    

    [Benchmark]
    public void RealisticWorkload()
    {
        foreach (var n in NumberOfLabelsSource())
        {
            RealisticWorkload(n);
        }
    }
    
    public void RealisticWorkload(int numberOfLabels)
    {
        var seedData = _seedDataByNumberOfLabels[numberOfLabels];
        var storage = Storage.Create();

        // 1000 Adds
        foreach (var entry in seedData.AddData)
        {
            storage.Add(entry);
        }

        // 100k Gets
        foreach (var (x, y) in seedData.GetCoordinates)
        {
            storage.Get(x, y);
        }

        // 1000 ListAll
        for (var i = 0; i < 100; i++)
        {
            var result = storage.ListAll();
        }

        // 10k GetInRegion
        foreach (var (minX, minY, maxX, maxY) in seedData.Regions)
        {
            var result = storage.GetInRegion(minX, minY, maxX, maxY);
        }

        // 10k GetWithinRadius
        foreach (var radius in seedData.Radii)
        {
            var result = storage.GetWithinRadius(radius);
        }

        // 10k GetWithinRadius from center point
        foreach (var (x, y, radius) in seedData.RadiiFromCenter)
        {
            var result = storage.GetWithinRadius(x, y, radius);
        }
    }
}
