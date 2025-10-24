namespace TreeMap;

/// <summary>
/// Generates test data for map label storage testing.
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// Generates a small set of manually curated test labels.
    /// </summary>
    public static IEnumerable<(int x, int y, string label)> GetBasicTestData()
    {
        yield return (1, 1, "label1");
        yield return (200, 3400, "label2");
        yield return (999999, 999999, "corner");
        yield return (500000, 500000, "center");
    }

    /// <summary>
    /// Generates a random set of labels for stress testing.
    /// </summary>
    /// <param name="count">Number of labels to generate</param>
    /// <param name="maxCoordinate">Maximum coordinate value</param>
    /// <param name="seed">Random seed for reproducibility</param>
    public static IEnumerable<(int x, int y, string label)> GenerateRandomLabels(
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
            
            yield return (x, y, label);
        }
    }

    /// <summary>
    /// Generates labels in a grid pattern for spatial testing.
    /// </summary>
    /// <param name="gridSize">Number of labels per axis (total = gridSize²)</param>
    /// <param name="maxCoordinate">Maximum coordinate value</param>
    public static IEnumerable<(int x, int y, string label)> GenerateGridPattern(
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
                
                yield return (x, y, label);
            }
        }
    }

    /// <summary>
    /// Generates labels along the edges and corners of the map.
    /// </summary>
    public static IEnumerable<(int x, int y, string label)> GenerateEdgeCases(int maxCoordinate = 1_000_000)
    {
        var max = maxCoordinate - 1;
        
        // Corners
        yield return (0, 0, "corner_bottom_left");
        yield return (max, 0, "corner_bottom_right");
        yield return (0, max, "corner_top_left");
        yield return (max, max, "corner_top_right");
        
        // Centers of edges
        yield return (max / 2, 0, "edge_bottom");
        yield return (max / 2, max, "edge_top");
        yield return (0, max / 2, "edge_left");
        yield return (max, max / 2, "edge_right");
        
        // Center
        yield return (max / 2, max / 2, "center");
    }

    /// <summary>
    /// Generates labels clustered in a specific region for spatial query testing.
    /// </summary>
    public static IEnumerable<(int x, int y, string label)> GenerateClusteredData(
        int count,
        int centerX,
        int centerY,
        int clusterRadius,
        int? seed = null)
    {
        var random = seed.HasValue ? new(seed.Value) : new Random();
        
        for (var i = 0; i < count; i++)
        {
            // Generate random offset within cluster radius
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * clusterRadius;
            
            var x = centerX + (int)(Math.Cos(angle) * distance);
            var y = centerY + (int)(Math.Sin(angle) * distance);
            
            // Ensure coordinates are valid
            x = Math.Max(0, Math.Min(999999, x));
            y = Math.Max(0, Math.Min(999999, y));
            
            var label = $"cluster_{i}";
            
            yield return (x, y, label);
        }
    }

    /// <summary>
    /// Generates a mixed dataset combining different patterns.
    /// </summary>
    public static IEnumerable<(int x, int y, string label)> GenerateMixedDataset(
        int totalCount,
        int? seed = null)
    {
        var labels = new List<(int x, int y, string label)>();
        
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

