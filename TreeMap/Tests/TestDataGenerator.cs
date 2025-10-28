namespace TreeMap;

/// <summary>
/// Container for benchmark seed data.
/// </summary>
public class BenchmarkSeedData
{
    public List<Entry> AddData { get; set; } = null!;
    public List<(int x, int y)> GetCoordinates { get; set; } = null!;
    public List<(int minX, int minY, int maxX, int maxY)> Regions { get; set; } = null!;
    public List<int> Radii { get; set; } = null!;
    public List<(int x, int y, int radius)> RadiiFromCenter { get; set; } = null!;
}

/// <summary>
/// Generates test data for map label storage testing.
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// Generates a small set of manually curated test labels.
    /// </summary>
    public static IEnumerable<Entry> GetBasicTestData()
    {
        yield return new(1, 1, "label1");
        yield return new(200, 3400, "label2");
        yield return new(999999, 999999, "corner");
        yield return new(500000, 500000, "center");
    }

    /// <summary>
    /// Generates a random set of labels for stress testing.
    /// </summary>
    /// <param name="count">Number of labels to generate</param>
    /// <param name="maxCoordinate">Maximum coordinate value</param>
    /// <param name="seed">Random seed for reproducibility</param>
    public static IEnumerable<Entry> GenerateRandomLabels(
        int count,
        int maxCoordinate = 1_000_000, 
        int? seed = null)
    {
        var random = seed.HasValue ? new(seed.Value) : new Random();

        for (var i = 0; i < count; i++)
        {
            var x = random.Next(0, maxCoordinate);
            var y = random.Next(0, maxCoordinate);
            var label = $"random_label_{i}";

            yield return new(x, y, label);
        }
    }

    /// <summary>
    /// Generates labels in a grid pattern for spatial testing.
    /// </summary>
    /// <param name="gridSize">Number of labels per axis (total = gridSize²)</param>
    /// <param name="maxCoordinate">Maximum coordinate value</param>
    public static IEnumerable<Entry> GenerateGridPattern(
        int gridSize,
        int maxCoordinate = 1_000_000)
    {
        var step = maxCoordinate / (gridSize + 1);

        for (var i = 1; i <= gridSize; i++)
        {
            for (var j = 1; j <= gridSize; j++)
            {
                var x = i * step;
                var y = j * step;
                var label = $"grid_{i}_{j}";

                yield return new(x, y, label);
            }
        }
    }

    /// <summary>
    /// Generates labels along the edges and corners of the map.
    /// </summary>
    public static IEnumerable<Entry> GenerateEdgeCases(int maxCoordinate = 1_000_000)
    {
        var max = maxCoordinate - 1;

        // Corners
        yield return new(0, 0, "corner_bottom_left");
        yield return new(max, 0, "corner_bottom_right");
        yield return new(0, max, "corner_top_left");
        yield return new(max, max, "corner_top_right");

        // Centers of edges
        yield return new(max / 2, 0, "edge_bottom");
        yield return new(max / 2, max, "edge_top");
        yield return new(0, max / 2, "edge_left");
        yield return new(max, max / 2, "edge_right");

        // Center
        yield return new(max / 2, max / 2, "center");
    }

    /// <summary>
    /// Generates labels clustered in a specific region for spatial query testing.
    /// </summary>
    public static IEnumerable<Entry> GenerateClusteredData(
        int count,
        int centerX,
        int centerY,
        int clusterRadius,
        int? seed = null)
    {
        var random = seed.HasValue ? new(seed.Value) : new Random();

        for (var i = 0; i < count; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * clusterRadius;

            var x = centerX + (int)(Math.Cos(angle) * distance);
            var y = centerY + (int)(Math.Sin(angle) * distance);

            x = Math.Max(0, Math.Min(999999, x));
            y = Math.Max(0, Math.Min(999999, y));
            
            var label = $"cluster_{i}";

            yield return new(x, y, label);
        }
    }

    /// <summary>
    /// Generates benchmark seed data including coordinates, regions, and radii for testing.
    /// </summary>
    public static BenchmarkSeedData GenerateBenchmarkSeedData(
        List<Entry> addData,
        int seed = 42)
    {
        var random = new Random(seed);
        var seedData = new BenchmarkSeedData
        {
            AddData = addData,
            GetCoordinates = [],
            Regions = [],
            Radii = [],
            RadiiFromCenter = []
        };

        // Prepare 100k get coordinates (mix of existing and non-existing)
        for (var i = 0; i < 1_000_000 / 10; i++)
        {
            // 50% existing, 50% random
            var item = i % seedData.AddData.Count < seedData.AddData.Count / 2
                ? (seedData.AddData[i % seedData.AddData.Count].X, seedData.AddData[i % seedData.AddData.Count].Y)
                : (random.Next(1_000_000), random.Next(1_000_000));

            seedData.GetCoordinates.Add(item);
        }

        // Prepare 10k regions for GetInRegion
        for (var i = 0; i < 10_000; i++)
        {
            var centerX = random.Next(1_000_000);
            var centerY = random.Next(1_000_000);
            var size = random.Next(50_000, 200_000);

            seedData.Regions.Add((
                Math.Max(0, centerX - size / 2),
                Math.Max(0, centerY - size / 2),
                Math.Min(999_999, centerX + size / 2),
                Math.Min(999_999, centerY + size / 2)
            ));
        }

        // Prepare 10k radii for GetWithinRadius
        for (var i = 0; i < 10_000; i++)
        {
            seedData.Radii.Add(random.Next(0, 800_000));
        }

        // Prepare 10k center points with radii for GetWithinRadius(x, y, r)
        for (var i = 0; i < 10_000; i++)
        {
            var centerX = random.Next(1_000_000);
            var centerY = random.Next(1_000_000);
            var radius = random.Next(1_000, 100_000);
            seedData.RadiiFromCenter.Add((centerX, centerY, radius));
        }

        return seedData;
    }

    /// <summary>
    /// Generates a mixed dataset combining different patterns.
    /// </summary>
    public static IEnumerable<Entry> GenerateMixedDataset(
        int totalCount,
        int? seed = null)
    {
        var labels = new List<Entry>();

        // Add edge cases (9 labels)
        labels.AddRange(GenerateEdgeCases());
        
        // Add some grid points (16 labels)
        labels.AddRange(GenerateGridPattern(4));
        
        // Fill the rest with random labels
        var remaining = totalCount - labels.Count;
        if (remaining > 0)
        {
            labels.AddRange(GenerateRandomLabels(remaining, seed: seed));
        }
        
        return labels;
    }
}
