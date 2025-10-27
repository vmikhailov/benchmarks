# MapStorage_Tiled Implementation Summary

## What Was Created

A new spatial hash grid implementation (`MapStorage_Tiled`) that optimizes spatial queries by dividing the map into power-of-2 tiles.

## Key Implementation Details

### Core Concept
Instead of storing all entries in a single flat dictionary, entries are organized into tiles:
- Map divided into 1024×1024 tiles (configurable via `tileShift` parameter)
- Each tile is a dictionary of entries within that tile's bounds
- Only occupied tiles are stored in memory (sparse representation)

### Data Structure
```
Dictionary<(tileX, tileY), Dictionary<(x, y), Entry>>
```

## Optimization Strategy

### 1. GetWithinRadius - The Big Win
Traditional approach (Dictionary/BST):
- Check every single entry against the circle equation
- Time: O(n) where n = total entries

Tiled approach:
1. **Bounding Box**: Calculate rectangular bounds of the circle
2. **Tile Range**: Determine which tiles intersect the bounding box
3. **Three-Way Classification**:
   - **Outside tiles**: Skip entirely (not in tile range)
   - **Completely inside tiles**: Add all entries without checking (fast path)
   - **Boundary tiles**: Filter entries by distance (only needed for tiles that partially overlap)

Example performance:
```
Query: GetWithinRadius(500000, 500000, 100000) on 1,000,000×1,000,000 map with 1000 entries

Dictionary: Check all 1000 entries individually
Tiled: 
- Tiles in bounding box: ~195×195 = 38,025 potential tiles
- Actually occupied tiles: ~1-40 (sparse map)
- Tiles completely inside circle: Add in bulk (fast)
- Boundary tiles: Filter individually
Result: Much faster, especially as entry count grows
```

### 2. GetInRegion - Similar Optimization
1. Calculate which tiles intersect the region
2. Tiles completely inside: bulk add (fast path)
3. Boundary tiles: filter by coordinates
4. Outside tiles: skip entirely

### 3. Basic Operations - Minimal Overhead
- Add/Get/Remove: Just one extra bit shift operation to find the tile
- Still O(1) average time
- Memory overhead: ~8 bytes per occupied tile

## Files Created

1. **MapStorage_Tiled.cs** - Main implementation
   - Implements IMapStorage interface
   - Power-of-2 tile sizing for efficient bit operations
   - Smart spatial query algorithms with fast/boundary paths

2. **TiledStorageTest.cs** - Correctness tests
   - Tests all basic operations
   - Tests spatial queries with various scenarios
   - Verifies tiles work correctly across boundaries

3. **Tiled.md** - Comprehensive documentation
   - Algorithm explanations
   - Performance characteristics
   - Usage guidelines and tuning advice

## Integration

Updated benchmark files to include the new implementation:
- `WeightedBenchmark.cs` - Added `MapStorage_Tiled` to comparison
- `StandardBenchmarks.cs` - Added `MapStorage_Tiled` to comparison

## Testing

All tests pass:
```
✓ Basic operations (Add, Get, Remove, Contains)
✓ GetInRegion with multiple scenarios
✓ GetWithinRadius from origin
✓ GetWithinRadius from center point
```

## Expected Performance Benefits

Based on the algorithm design, expected improvements:

1. **GetWithinRadius**: 
   - Dictionary/BST: O(n) - must check every entry
   - Tiled: O(t + m) - only check relevant tiles and boundary entries
   - Expected: 5-20x faster for typical sparse maps

2. **GetInRegion**:
   - Similar improvements as GetWithinRadius
   - Benefit scales with region size vs. data density

3. **Basic operations**:
   - Negligible overhead (one bit shift)
   - Still O(1) average

4. **Memory**:
   - Same O(n) as Dictionary
   - Small overhead for tile dictionary keys

## Usage

```csharp
// Default: 1024×1024 tiles
var storage = new MapStorage_Tiled();

// Custom tile size (512×512)
var storage = new MapStorage_Tiled(maxCoordinate: 1_000_000, tileShift: 9);

// Use like any other IMapStorage
storage.Add(new Entry(100, 200, "Label"));
var nearby = storage.GetWithinRadius(500000, 500000, 100000);
```

## Why This Helps

The key insight is that for sparse maps with spatial queries:
- Most tiles are empty (can skip entirely)
- Some tiles are completely inside the query area (bulk add without checking)
- Only boundary tiles need individual entry filtering

This three-way classification (outside/inside/boundary) is what makes the tiled approach so efficient for spatial queries.

