using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace TreeMap.Benchmarks;

/// <summary>
/// Example benchmark showing how to use custom aggregation columns.
/// Note: Custom grouping rules are NOT supported - BenchmarkLogicalGroupRule is an enum.
/// You can use ByMethod, ByParams, ByCategory, or ByJob from the enum.
/// </summary>
[Config(typeof(Config))]
[MemoryDiagnoser]
[RankColumn]
public class WeightedBenchmarkWithCustomAggregation
{
    private class Config : ManualConfig
    {
        public Config()
        {
            // Use built-in grouping rules (enum values only):
            // - Group by method to compare storage types across all data sizes
            AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByMethod);

            // Alternative: Group by params to see each parameter combination separately
            // AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByParams);

            // Add custom columns for aggregation
            AddColumn(new GeometricMeanAcrossDataSizesColumn());
            AddColumn(new AverageAcrossDataSizesColumn());

            // Add weighted score with custom weights (e.g., small datasets matter more)
            var weights = new Dictionary<int, double>
            {
                { 1000, 0.5 },    // 50% weight for small dataset
                { 10000, 0.3 },   // 30% weight for medium dataset
                { 50000, 0.2 }    // 20% weight for large dataset
            };
            AddColumn(new WeightedScoreColumn(weights));

            // Order by fastest to slowest within each group
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

            HideColumns("Error", "StdDev");
            AddJob(Job.Default
                .WithWarmupCount(1)
                .WithIterationCount(5));
        }
    }

    private List<Entry> _addData = null!;
    private List<(int x, int y)> _getCoordinates = null!;
    private List<(int minX, int minY, int maxX, int maxY)> _regions = null!;
    private List<int> _radii = null!;
    private List<(int x, int y, int radius)> _radiiFromCenter = null!;

    [ParamsSource(nameof(StorageTypes))]
    public IMapStorageFactory Storage { get; set; } = null!;

    [Params(1000, 10000, 50000)]
    public int NumberOfLabels { get; set; }

    public static IMapStorageFactory[] StorageTypes =>
    [
        new StorageFactory<MapStorage_Tiled>("Tiled_15", 1_000_000, 15),
        new StorageFactory<MapStorage_Tiled>("Tiled_16", 1_000_000, 16),
        new StorageFactory<MapStorage_Tiled>("Tiled_17", 1_000_000, 17)
    ];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _addData = TestDataGenerator.GenerateRandomLabels(NumberOfLabels, seed: 42).ToList();

        _getCoordinates = new();
        for (var i = 0; i < 1_000_000/10; i++)
        {
            if (i < 50_000 && i < _addData.Count)
            {
                _getCoordinates.Add((_addData[i % _addData.Count].X, _addData[i % _addData.Count].Y));
            }
            else
            {
                _getCoordinates.Add((random.Next(1_000_000), random.Next(1_000_000)));
            }
        }

        _regions = new();
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

        _radii = new();
        for (var i = 0; i < 10_000; i++)
        {
            _radii.Add(random.Next(0, 800_000));
        }

        _radiiFromCenter = new();
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

        foreach (var entry in _addData)
        {
            storage.Add(entry);
        }

        foreach (var (x, y) in _getCoordinates)
        {
            storage.Get(x, y);
        }

        for (var i = 0; i < 100; i++)
        {
            _ = storage.ListAll();
        }

        foreach (var (minX, minY, maxX, maxY) in _regions)
        {
            _ = storage.GetInRegion(minX, minY, maxX, maxY);
        }

        foreach (var radius in _radii)
        {
            _ = storage.GetWithinRadius(radius);
        }

        foreach (var (x, y, radius) in _radiiFromCenter)
        {
            _ = storage.GetWithinRadius(x, y, radius);
        }
    }
}

