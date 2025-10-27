# GetWithinRadius Update

## Summary

Added a new overload to the `GetWithinRadius` method that allows querying for all labels within a radius from an arbitrary center point (x, y), not just from the origin (0, 0).

## Changes Made

### 1. Interface Update (`IMapStorage.cs`)

Added new method signature:
```csharp
Entry[] GetWithinRadius(int centerX, int centerY, int radius);
```

This method returns all labels where `(x - centerX)² + (y - centerY)² <= radius²`.

### 2. Implementation Updates

All storage implementations have been updated:

- **MapStorage_Dictionary** - Linear scan through all entries
- **MapStorage_StringKey** - Linear scan through all entries
- **MapStorage_BST** - Linear scan (cannot optimize with BST structure for arbitrary center)
- **MapStorage_SortedArray** - Linear scan (cannot optimize with binary search for arbitrary center)
- **MapStorage_SortedDictionary** - Linear scan through all entries
- **MapStorage_SortedDictionary2** - Linear scan through all entries

**Note**: For queries from origin (0,0), the optimized BST and SortedArray implementations can use their data structures efficiently. However, for arbitrary center points, all implementations must check every entry, making them O(n).

### 3. Benchmark Updates

#### WeightedBenchmark
Updated the realistic workload benchmark to include both radius query types:
- 50,000 `GetWithinRadius(radius)` calls - from origin (0,0)
- 50,000 `GetWithinRadius(centerX, centerY, radius)` calls - from random centers

#### MapStorageBenchmarks
Added new benchmark method:
```csharp
[Benchmark]
[BenchmarkCategory("GetWithinRadiusFromCenter")]
public void GetWithinRadiusFromCenter()
```

This tests 100 random queries with different center points and radii.

## Performance Results

### Weighted Benchmark Results (with new method included)

The benchmark now tests: 1000 adds, 100k gets, 1000 ListAll, 10k GetInRegion, 50k GetWithinRadius (from origin), 50k GetWithinRadius (from center).

| Storage      | Mean      | Rank | Allocated  |
|--------------|-----------|------|------------|
| SortedArray  | 208.9 ms  | 1    | 228.12 MB  |
| Dictionary   | 302.2 ms  | 2    | 447.42 MB  |
| StringKey    | 320.0 ms  | 3    | 493.24 MB  |
| SortedDict   | 1,072.6 ms| 4    | 466.92 MB  |
| BST          | 1,409.7 ms| 5    | 1660.93 MB |

**Key Findings:**
- SortedArray remains the fastest for this mixed workload
- StringKey storage is ~6% slower than Dictionary (320 ms vs 302 ms) due to string allocation overhead
- The new center-based radius queries don't provide optimization opportunities for tree-based structures

## Use Cases

The new method is useful for:
- Finding all labels near a specific landmark (not just origin)
- Dynamic radius searches where the center point changes
- Spatial queries for game objects, geographic data, etc.

## Example Usage

```csharp
var storage = new MapStorage_Dictionary();

// Add some entries
storage.Add(new Entry(100, 100, "label1"));
storage.Add(new Entry(150, 150, "label2"));
storage.Add(new Entry(500, 500, "label3"));

// Find all entries within radius 100 from point (120, 120)
var nearbyLabels = storage.GetWithinRadius(120, 120, 100);
// Returns label1 and label2
```

## Implementation Notes

- All implementations validate the center coordinates to ensure they're within the map bounds
- Radius must be non-negative
- Uses `long` arithmetic to avoid integer overflow: `(long)dx * dx + (long)dy * dy`
- Comparison uses `<=` to include entries exactly at the radius boundary

