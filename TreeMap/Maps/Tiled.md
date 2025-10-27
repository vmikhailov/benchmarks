# MapStorage_Tiled - Spatial Hash Grid Implementation

## Overview

`MapStorage_Tiled` is an optimized spatial data structure that divides the 2D map into a grid of tiles (power-of-2 sized squares). This approach significantly improves performance for spatial queries like `GetWithinRadius` and `GetInRegion` by quickly eliminating entire tiles that don't intersect the query area.

## Key Features

### Tile-Based Organization
- The map is divided into tiles of size 2^n × 2^n (default: 1024 × 1024)
- Only tiles containing entries are stored in memory (sparse representation)
- Each tile contains a dictionary of entries within that tile

### Performance Benefits

#### 1. **GetWithinRadius** - Major Improvement
The tiled approach provides significant speedup for radius queries:
- **Bounding Box Calculation**: First, calculate the rectangular bounding box of the circle
- **Tile Filtering**: Only check tiles that intersect the bounding box
- **Fast Path**: If a tile is completely inside the circle (all 4 corners inside), add all entries without checking
- **Boundary Path**: Only filter entries in tiles that partially overlap the circle

Example:
```
For radius 100,000 on a 1,000,000×1,000,000 map with 1024×1024 tiles:
- Dictionary approach: Check all ~1000 entries
- Tiled approach: 
  * Bounding box covers ~195×195 tiles = 38,025 potential tiles
  * Only ~0.001 tiles actually contain data (very sparse)
  * Check ~38 non-empty tiles
  * Most tiles on boundary require filtering
  * Tiles completely inside: fast bulk add
```

#### 2. **GetInRegion** - Significant Improvement
Similar optimization for rectangular region queries:
- Calculate which tiles intersect the region
- Tiles completely inside: add all entries (fast path)
- Tiles on boundary: filter entries
- Skip all tiles outside the region

#### 3. **Basic Operations** - Minimal Overhead
- **Add/Get/Remove**: O(1) with minimal overhead (one extra bit shift to find tile)
- **Memory**: Only tiles with entries are allocated (sparse)

## Implementation Details

### Data Structure
```csharp
Dictionary<(int tileX, int tileY), Dictionary<(int x, int y), Entry>>
```
- Outer dictionary: Maps tile coordinates to tile contents
- Inner dictionary: Maps entry coordinates to entries (same as MapStorage_Dictionary)

### Tile Calculation
```csharp
tileX = x >> tileShift  // Equivalent to: x / tileSize
tileY = y >> tileShift  // Fast bit shift operation
```

### Configuration
Default: `tileShift = 10` (1024×1024 tiles)
- For 1,000,000×1,000,000 map: ~977×977 = 954,529 potential tiles
- With 1,000 labels: Average ~0.001 labels per tile (very sparse)
- Only occupied tiles are stored in memory

### Spatial Query Algorithm

#### GetWithinRadius(centerX, centerY, radius)
```
1. Calculate bounding box: [minX, minY] to [maxX, maxY]
2. Calculate tile range: [minTileX, minTileY] to [maxTileX, maxTileY]
3. For each tile in range:
   a. Skip if tile doesn't exist
   b. Check if all 4 corners are inside circle:
      - YES: Add all entries from tile (fast path)
      - NO: Filter entries by distance (boundary path)
```

#### GetInRegion(minX, minY, maxX, maxY)
```
1. Calculate tile range from region bounds
2. For each tile in range:
   a. Skip if tile doesn't exist
   b. Check if tile is completely inside region:
      - YES: Add all entries from tile (fast path)
      - NO: Filter entries by coordinates (boundary path)
```

## Performance Characteristics

### Time Complexity
- **Add**: O(1) average
- **Get**: O(1) average
- **Remove**: O(1) average
- **GetWithinRadius**: O(t + m) where:
  - t = number of tiles checked (much smaller than total entries)
  - m = entries in boundary tiles that need filtering
- **GetInRegion**: O(t + m) where:
  - t = number of tiles in region
  - m = entries in boundary tiles that need filtering

### Space Complexity
- O(n) where n = number of entries
- Additional overhead: ~8 bytes per occupied tile for tile dictionary key

## Comparison with Other Implementations

| Implementation | GetWithinRadius | GetInRegion | Add/Get | Memory |
|---------------|----------------|-------------|---------|---------|
| Dictionary    | O(n) - check all | O(n) - check all | O(1) | O(n) |
| BST           | O(n) - tree traversal | O(n) - tree traversal | O(log n) | O(n) |
| **Tiled**     | **O(t+m) - only relevant tiles** | **O(t+m) - only relevant tiles** | O(1) | O(n) |

Where:
- n = total entries
- t = tiles checked (typically << n for sparse maps)
- m = entries in boundary tiles

## When to Use

### Best For:
- Sparse maps (few entries relative to map size)
- Frequent spatial queries (GetWithinRadius, GetInRegion)
- Large query areas that can benefit from tile-level culling
- Scenarios where entries are somewhat clustered

### Not Ideal For:
- Extremely dense maps (most tiles occupied)
- Maps where all entries need to be checked anyway
- Very small query areas (overhead of tile checking not worth it)

## Tuning Tile Size

The tile size (2^tileShift) can be adjusted:
- **Smaller tiles** (e.g., 512×512, shift=9):
  - Better for small query areas
  - More tiles = more tile dictionary overhead
  - Finer granularity for culling
  
- **Larger tiles** (e.g., 2048×2048, shift=11):
  - Better for large query areas
  - Fewer tiles = less tile dictionary overhead
  - Less granular culling (more boundary filtering)

Default 1024×1024 (shift=10) is a good balance for the 1,000,000×1,000,000 map with ~1000 entries.

## Example Usage

```csharp
// Create with default settings (1024×1024 tiles)
var storage = new MapStorage_Tiled();

// Or customize tile size
var storage = new MapStorage_Tiled(maxCoordinate: 1_000_000, tileShift: 9); // 512×512 tiles

// Use exactly like other IMapStorage implementations
storage.Add(new Entry(100, 200, "Label1"));
var entries = storage.GetWithinRadius(500_000, 500_000, 100_000);
```

## Benchmarking

Run benchmarks to compare with other implementations:
```bash
dotnet run --configuration Release -- benchmark weighted
```

The tiled approach should show significant improvements for:
- GetWithinRadius operations
- GetInRegion operations
- Especially when query areas are moderate to large relative to data density

