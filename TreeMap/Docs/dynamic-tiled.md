# MapStorage_DynamicTiled Implementation

## Overview

`MapStorage_DynamicTiled` is an adaptive spatial data structure that automatically splits tiles when they reach a configurable capacity limit. Unlike the fixed-grid `MapStorage_Tiled`, this implementation adjusts its structure based on actual data distribution.

## Key Features

### Adaptive Splitting
- Starts with a single tile covering the entire 1,000,000 x 1,000,000 map
- When a tile reaches capacity, it splits along its **longest dimension**
- Automatically creates more tiles in dense areas, fewer in sparse areas
- Splits are recursive if needed after redistribution

### Configurable Capacity
- Default: 64 entries per tile
- Can be configured via constructor: `new MapStorage_DynamicTiled(1_000_000, capacity)`
- Lower capacity = more tiles, better spatial locality
- Higher capacity = fewer tiles, less overhead

### Splitting Strategy
```csharp
if (width >= height)
    // Split horizontally at midpoint
else
    // Split vertically at midpoint
```

## Performance Characteristics

### Time Complexity
- **Add**: O(1) average, O(log n) when splitting occurs
- **Get**: O(t) where t = number of tiles (typically small)
- **Remove**: O(t) average
- **GetInRegion**: O(t + m) where t = tiles checked, m = matching entries
- **GetWithinRadius**: O(t + m) similar to region queries

### Space Complexity
- O(n + t) where n = entries, t = tiles
- Tile count depends on data distribution and capacity
- For 1000 clustered entries with capacity 32: ~49 tiles
- For 1000 sparse entries with capacity 32: ~44 tiles

## Comparison with MapStorage_Tiled

| Feature | MapStorage_Tiled | MapStorage_DynamicTiled |
|---------|------------------|-------------------------|
| Tile Structure | Fixed power-of-2 grid | Adaptive rectangles |
| Tile Lookup | O(1) coordinate math | O(t) search all tiles |
| Memory | Sparse dictionary | Variable tile count |
| Adaptation | No | Yes, based on data |
| Best For | Uniform distribution | Clustered data |

## Usage Examples

### Basic Usage
```csharp
// Default: 1M x 1M map, capacity 64
var storage = new MapStorage_DynamicTiled();

// Custom capacity
var storage = new MapStorage_DynamicTiled(1_000_000, 32);
```

### Testing
```bash
# Test with default capacity (64)
dotnet run -- test dynamic

# Test with custom capacity (32)
dotnet run -- test dynamic 32

# Test with performance benchmarks
dotnet run -- test dynamic 64 perf
```

### Benchmarking
The implementation is included in `WeightedBenchmark` with three variants:
- `Dynamic_32`: Capacity 32 (more splits)
- `Dynamic_64`: Capacity 64 (balanced)
- `Dynamic_128`: Capacity 128 (fewer splits)

```bash
dotnet run --configuration Release -- benchmark weighted
```

## Capacity Selection Guide

### Small Capacity (16-32)
- **Pros**: Better spatial locality, faster range queries for clustered data
- **Cons**: More overhead, slower lookups (more tiles to check)
- **Best for**: Highly clustered data, frequent range queries

### Medium Capacity (64-128)
- **Pros**: Balanced performance
- **Cons**: None significant
- **Best for**: General purpose, mixed workloads

### Large Capacity (256+)
- **Pros**: Fewer tiles, faster point queries
- **Cons**: Less spatial optimization, slower range queries
- **Best for**: Sparse data, mostly point queries

## Performance Results

From testing with 1000 entries:

| Capacity | Clustered Tiles | Sparse Tiles | Add Time |
|----------|----------------|--------------|----------|
| 16       | 96             | ~90          | ~1ms     |
| 32       | 49             | ~44          | ~0ms     |
| 64       | 29             | ~25          | ~1ms     |
| 128      | 17             | ~15          | ~0ms     |
| 256      | 11             | ~10          | ~1ms     |

## Implementation Details

### Tile Class
```csharp
private class Tile
{
    public int Id { get; }
    public int MinX, MinY, MaxX, MaxY { get; } // Rectangle bounds
    public Dictionary<(int x, int y), Entry> Entries { get; }
}
```

### Finding Tiles
Unlike fixed-grid tiling, finding a tile requires iterating through all tiles:
```csharp
private Tile? FindTile(int x, int y)
{
    foreach (var tile in _tiles.Values)
    {
        if (tile.Contains(x, y))
            return tile;
    }
    return null;
}
```

This is acceptable because:
1. Tile count remains relatively small (< 100 for typical workloads)
2. Modern CPU caching makes linear search fast for small collections
3. The benefit of adaptive splitting outweighs the lookup cost

### Optimization: Fast Path
For range queries, entire tiles can be accepted without checking individual entries:
```csharp
if (tileCompletelyInside)
{
    result.AddRange(tile.Entries.Values);  // Fast path!
}
```

## When to Use

### Choose MapStorage_DynamicTiled When:
- Data has unknown or varying distribution
- Data is highly clustered
- You want automatic optimization
- Memory is not extremely constrained

### Choose MapStorage_Tiled When:
- Data is uniformly distributed
- You need predictable O(1) lookups
- You know the optimal tile size in advance
- You want minimal overhead

### Choose Other Implementations When:
- **Dictionary**: Fastest point queries, no spatial optimization
- **SortedArray**: Memory-constrained environments
- **BST**: Ordered iteration needed

## Debugging

The implementation exposes `TileCount` property for monitoring:
```csharp
var storage = new MapStorage_DynamicTiled(1_000_000, 64);
// Add entries...
Console.WriteLine($"Current tiles: {storage.TileCount}");
```

This helps understand how the structure adapts to your data.

