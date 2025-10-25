namespace TreeMap;

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
        yield return new Entry(1, 1, "label1");
        yield return new Entry(200, 3400, "label2");
        yield return new Entry(999999, 999999, "corner");
        yield return new Entry(500000, 500000, "center");
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
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        for (var i = 0; i < count; i++)
        {
            var x = random.Next(0, maxCoordinate);
            var y = random.Next(0, maxCoordinate);
            var label = $"random_label_{i}";

            yield return new Entry(x, y, label);
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

                yield return new Entry(x, y, label);
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
        yield return new Entry(0, 0, "corner_bottom_left");
        yield return new Entry(max, 0, "corner_bottom_right");
        yield return new Entry(0, max, "corner_top_left");
        yield return new Entry(max, max, "corner_top_right");

        // Centers of edges
        yield return new Entry(max / 2, 0, "edge_bottom");
        yield return new Entry(max / 2, max, "edge_top");
        yield return new Entry(0, max / 2, "edge_left");
        yield return new Entry(max, max / 2, "edge_right");

        // Center
        yield return new Entry(max / 2, max / 2, "center");
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
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        for (var i = 0; i < count; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * clusterRadius;

            var x = centerX + (int)(Math.Cos(angle) * distance);
            var y = centerY + (int)(Math.Sin(angle) * distance);

            x = Math.Max(0, Math.Min(999999, x));
            y = Math.Max(0, Math.Min(999999, y));
            
            var label = $"cluster_{i}";

            yield return new Entry(x, y, label);
        }
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
