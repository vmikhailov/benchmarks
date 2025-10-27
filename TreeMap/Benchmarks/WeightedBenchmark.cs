using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace TreeMap;

/// <summary>
/// Weighted benchmark that simulates realistic usage patterns.
/// Operations: 1000 adds, 100k gets, 1000 ListAll, 10k GetInRegion, 10k GetWithinRadius
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Error", "StdDev")]
//[SimpleJob(warmupCount: 1, iterationCount: 5)]
public class WeightedBenchmark
{
    private List<Entry> _addData = null!;
    private List<(int x, int y)> _getCoordinates = null!;
    private List<(int minX, int minY, int maxX, int maxY)> _regions = null!;
    private List<int> _radii = null!;
    private List<(int x, int y, int radius)> _radiiFromCenter = null!;

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    //[Params(1000)]
    [Params(1000, 10000, 50000)]
    public int NumberOfLabels { get; set; }

    public static IMapStorageFactory[] StorageTypes =>
    [
//        new StorageFactory<MapStorage_BST>("BST"),
//        new StorageFactory<MapStorage_Dictionary>("DictLongKey"),
//        new StorageFactory<MapStorage_SortedArray>("SortedArray"),
        new StorageFactory<MapStorage_Tiled>("Tiled_15", 1_000_000, 15),
        new StorageFactory<MapStorage_Tiled>("Tiled_16", 1_000_000, 16),
        new StorageFactory<MapStorage_Tiled>("Tiled_17", 1_000_000, 17),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_32", 1_000_000, 32),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_64", 1_000_000, 64),
        new StorageFactory<MapStorage_DynamicTiled>("Dynamic_128", 1_000_000, 128),
//      new StorageFactory<MapStorage_SortedDictionary>("SortedDict"),
//      new StorageFactory<MapStorage_StringKey>("DictStringKey"),
    ];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);

        // Prepare 1000 entries to add
        _addData = TestDataGenerator.GenerateRandomLabels(NumberOfLabels, seed: 42).ToList();

        // Prepare 100k get coordinates (mix of existing and non-existing)
        _getCoordinates = [];
        for (var i = 0; i < 1_000_000/10; i++)
        {
            // 50% existing, 50% random
            var item = i % _addData.Count < _addData.Count / 2
                ? (_addData[i % _addData.Count].X, _addData[i % _addData.Count].Y)
                : (random.Next(1_000_000), random.Next(1_000_000));

            _getCoordinates.Add(item);
        }

        // Prepare 10k regions for GetInRegion
        _regions = [];
        for (var i = 0; i < 10_000; i++)
        {
            var centerX = random.Next(1_000_000);
            var centerY = random.Next(1_000_000);
            var size = random.Next(50_000, 200_000);

            _regions.Add((
                Math.Max(0, centerX - size / 2),
                Math.Max(0, centerY - size / 2),
                Math.Min(999_999, centerX + size / 2),
                Math.Min(999_999, centerY + size / 2)
            ));
        }

        // Prepare 10k radii for GetWithinRadius
        _radii = [];
        for (var i = 0; i < 10_000; i++)
        {
            _radii.Add(random.Next(0, 800_000));
        }

        // Prepare 10k center points with radii for GetWithinRadius(x, y, r)
        _radiiFromCenter = [];
        for (var i = 0; i < 10_000; i++)
        {
            var centerX = random.Next(1_000_000);
            var centerY = random.Next(1_000_000);
            var radius = random.Next(1_000, 100_000);
            _radiiFromCenter.Add((centerX, centerY, radius));
        }
    }

    [Benchmark]
    public void RealisticWorkload()
    {
        var storage = Storage.Create();

        // 1000 Adds
        foreach (var entry in _addData)
        {
            storage.Add(entry);
        }

        // 100k Gets
        foreach (var (x, y) in _getCoordinates)
        {
            storage.Get(x, y);
        }

        // 1000 ListAll
        for (var i = 0; i < 100; i++)
        {
            var result = storage.ListAll();
        }

        // 10k GetInRegion
        foreach (var (minX, minY, maxX, maxY) in _regions)
        {
            var result = storage.GetInRegion(minX, minY, maxX, maxY);
        }

        // 10k GetWithinRadius
        foreach (var radius in _radii)
        {
            var result = storage.GetWithinRadius(radius);
        }

        // 10k GetWithinRadius from center point
        foreach (var (x, y, radius) in _radiiFromCenter)
        {
            var result = storage.GetWithinRadius(x, y, radius);
        }
    }
}
