# MapStorage_Tiled Hang Fix

## Problem
The `MapStorage_Tiled` implementation was hanging during the weighted benchmark when testing `GetWithinRadius` with large radii (100,000 to 800,000).

## Root Cause
The original implementation used nested `for` loops to iterate over ALL possible tile coordinates in the bounding box:

```csharp
// PROBLEMATIC CODE (caused hang)
for (var tileX = minTileX; tileX <= maxTileX; tileX++)
{
    for (var tileY = minTileY; tileY <= maxTileY; tileY++)
    {
        if (!_tiles.TryGetValue((tileX, tileY), out var tile))
            continue;
        // ...process tile
    }
}
```

### Why This Caused a Hang

For a large radius like 800,000 on a 1,000,000×1,000,000 map with 1024×1024 tiles (tileShift=10):
- Bounding box: (0, 0) to (999,999, 999,999) after clamping
- Tile range: (0, 0) to (976, 976)
- **Total tile coordinates to check: 977 × 977 = 954,529 tiles!**

Even though most tiles are empty (only ~1000 entries in the sparse map), the nested loops still had to iterate through nearly 1 million tile coordinate pairs, calling `TryGetValue` on each one.

With 50,000 radius queries in the benchmark, this became:
- 50,000 queries × 954,529 tile checks = **47.7 BILLION dictionary lookups**
- This would take many minutes to hours to complete

## Solution
Changed from iterating over tile coordinates to iterating over only the tiles that EXIST in the dictionary:

```csharp
// FIXED CODE (fast)
foreach (var (tileKey, tile) in _tiles)
{
    var (tileX, tileY) = tileKey;
    
    // Skip tiles outside the bounding box
    if (tileX < minTileX || tileX > maxTileX || tileY < minTileY || tileY > maxTileY)
        continue;
    
    // ...process tile
}
```

### Why This Fixes It

For the same scenario with 1000 entries:
- Total existing tiles: ~1-1000 (depending on entry clustering)
- For each query: Check only ~1-1000 tiles instead of 954,529
- **Speedup: ~1000x to 954,000x!**

With 50,000 radius queries:
- 50,000 queries × ~1000 tiles = **~50 million checks** (vs 47.7 billion)
- Completes in seconds instead of hours

## Additional Fixes

1. **Constructor Parameter Order**: Fixed optional parameter ordering (all optional parameters must be at the end)
2. **Distance Check Consistency**: Fixed inequality operators for consistency (`<=` vs `<` for radius checks)

## Files Modified

- `TreeMap/Maps/MapStorage_Tiled.cs`
  - `GetWithinRadius(int radius)` - Changed from nested for loops to foreach over existing tiles
  - `GetWithinRadius(int centerX, int centerY, int radius)` - Same optimization
  - Constructor parameter order fixed

## Testing

All tests pass:
- ✓ Basic operations (Add, Get, Remove, Contains)
- ✓ GetInRegion with multiple scenarios  
- ✓ GetWithinRadius from origin
- ✓ GetWithinRadius from center point

Performance tests with large radii (100k-800k) complete in milliseconds instead of hanging.

## Key Takeaway

When working with sparse data structures (where only a small fraction of possible keys exist):
- **DON'T**: Iterate over all possible key coordinates and check for existence
- **DO**: Iterate over only the keys that exist and filter them

This is especially critical when:
- The space of possible keys is huge (e.g., 1 trillion positions on a 1M×1M map)
- The actual data is sparse (e.g., only 1000 entries)
- The operation is repeated many times (e.g., 50,000 queries in a benchmark)

