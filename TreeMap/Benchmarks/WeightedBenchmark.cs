using BenchmarkDotNet.Attributes;
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
[SimpleJob(warmupCount: 1, iterationCount: 5)]
public class WeightedBenchmark
{
    private List<Entry> _addData = null!;
    private List<(int x, int y)> _getCoordinates = null!;
    private List<(int minX, int minY, int maxX, int maxY)> _regions = null!;
    private List<int> _radii = null!;

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    public static IMapStorageFactory[] StorageTypes =>
    [
        new StorageFactory<MapStorage_Dictionary>("Dictionary"),
        new StorageFactory<MapStorage_BST>("BST"),
        new StorageFactory<MapStorage_SortedArray>("SortedArray"),
        new StorageFactory<MapStorage_SortedDictionary>("SortedDict"),
        //new StorageFactory<MapStorage_SortedDictionary2>("SortedDict2")
    ];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);

        // Prepare 1000 entries to add
        _addData = TestDataGenerator.GenerateRandomLabels(1000, seed: 42).ToList();

        // Prepare 100k get coordinates (mix of existing and non-existing)
        _getCoordinates = new();
        for (var i = 0; i < 1_000_000; i++)
        {
            // 50% existing, 50% random
            if (i < 50_000 && i < _addData.Count)
            {
                _getCoordinates.Add((_addData[i % _addData.Count].X, _addData[i % _addData.Count].Y));
            }
            else
            {
                _getCoordinates.Add((random.Next(1_000_000), random.Next(1_000_000)));
            }
        }

        // Prepare 10k regions for GetInRegion
        _regions = new();
        for (var i = 0; i < 20_000; i++)
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
        _radii = new();
        for (var i = 0; i < 50_000; i++)
        {
            _radii.Add(random.Next(100_000, 800_000));
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
        for (var i = 0; i < 1000; i++)
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
    }
}
