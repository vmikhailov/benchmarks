namespace TreeMap;

/// <summary>
/// Efficient in-memory storage for labels on a sparse 2D map using spatial tiling.
/// Optimized for ~100-1000 labels on a 1,000,000 x 1,000,000 map.
///
/// Data Structure: Dictionary of tiles (power-of-2 grid), each tile contains a dictionary of entries
/// Time Complexity:
///   - Add: O(1) average
///   - Get: O(1) average
///   - Remove: O(1) average
///   - GetWithinRadius: O(t + m) where t = tiles checked, m = entries in those tiles
///   - GetInRegion: O(t + m) where t = tiles in region, m = entries in those tiles
/// Space Complexity: O(n) where n = number of labels
///
/// The tiling approach allows us to:
/// 1. Quickly skip entire tiles that are outside the query area
/// 2. Accept entire tiles that are completely inside the query area
/// 3. Only filter entries in tiles that are on the boundary
/// </summary>
public class MapStorage_Tiled : IMapStorage
{
    private readonly Dictionary<(int tileX, int tileY), Dictionary<(int x, int y), Entry>> _tiles;
    private readonly int _maxCoordinate;
    private readonly int _tileShift;  // Power of 2 for tile size (e.g., 10 = 1024)
    private readonly int _tileSize;   // Actual tile size in coordinates
    private readonly int _tileMask;   // Mask for getting position within tile
    private int _count;

    /// <summary>
    /// Initializes a new tiled map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    /// <param name="tileShift">Power of 2 for tile size (default: 10, meaning 1024x1024 tiles)</param>
    public MapStorage_Tiled(int maxCoordinate, int tileShift)
    {
        _tiles = new();
        _maxCoordinate = maxCoordinate;
        _tileShift = tileShift;
        _tileSize = 1 << tileShift;  // 2^tileShift
        _tileMask = _tileSize - 1;
        _count = 0;
    }

    public MapStorage_Tiled() : this(1_000_000, 15)
    {
    }


    /// <summary>
    /// Gets the tile coordinates for a given position.
    /// </summary>
    private (int tileX, int tileY) GetTileCoords(int x, int y)
    {
        return (x >> _tileShift, y >> _tileShift);
    }

    /// <summary>
    /// Gets or creates a tile at the specified tile coordinates.
    /// </summary>
    private Dictionary<(int x, int y), Entry> GetOrCreateTile(int tileX, int tileY)
    {
        var tileKey = (tileX, tileY);
        if (!_tiles.TryGetValue(tileKey, out var tile))
        {
            tile = new Dictionary<(int x, int y), Entry>();
            _tiles[tileKey] = tile;
        }
        return tile;
    }

    public bool Add(Entry entry)
    {
        ValidateCoordinates(entry.X, entry.Y);

        var (tileX, tileY) = GetTileCoords(entry.X, entry.Y);
        var tile = GetOrCreateTile(tileX, tileY);
        var key = (entry.X, entry.Y);
        var isNew = !tile.ContainsKey(key);
        tile[key] = entry;

        if (isNew)
            _count++;

        return isNew;
    }

    public Entry? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var (tileX, tileY) = GetTileCoords(x, y);
        if (_tiles.TryGetValue((tileX, tileY), out var tile))
        {
            return tile.GetValueOrDefault((x, y));
        }
        return null;
    }

    public bool TryGet(int x, int y, out Entry? entry)
    {
        ValidateCoordinates(x, y);

        var (tileX, tileY) = GetTileCoords(x, y);
        if (_tiles.TryGetValue((tileX, tileY), out var tile))
        {
            return tile.TryGetValue((x, y), out entry);
        }
        entry = null;
        return false;
    }

    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);

        var (tileX, tileY) = GetTileCoords(x, y);
        if (_tiles.TryGetValue((tileX, tileY), out var tile))
        {
            if (tile.Remove((x, y)))
            {
                _count--;
                // Clean up empty tiles
                if (tile.Count == 0)
                {
                    _tiles.Remove((tileX, tileY));
                }
                return true;
            }
        }
        return false;
    }

    public bool Contains(int x, int y)
    {
        ValidateCoordinates(x, y);

        var (tileX, tileY) = GetTileCoords(x, y);
        return _tiles.TryGetValue((tileX, tileY), out var tile) && tile.ContainsKey((x, y));
    }

    public Entry[] ListAll()
    {
        var result = new Entry[_count];
        var index = 0;

        foreach (var tile in _tiles.Values)
        {
            foreach (var entry in tile.Values)
            {
                result[index++] = entry;
            }
        }

        return result;
    }

    public Entry[] GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        var result = new List<Entry>();

        // Calculate tile range
        var minTileX = minX >> _tileShift;
        var minTileY = minY >> _tileShift;
        var maxTileX = maxX >> _tileShift;
        var maxTileY = maxY >> _tileShift;

        // Iterate only over tiles that intersect the region
        for (var tileX = minTileX; tileX <= maxTileX; tileX++)
        {
            for (var tileY = minTileY; tileY <= maxTileY; tileY++)
            {
                if (!_tiles.TryGetValue((tileX, tileY), out var tile))
                    continue;

                // Check if tile is completely inside the region
                var tileBoundsMinX = tileX << _tileShift;
                var tileBoundsMinY = tileY << _tileShift;
                var tileBoundsMaxX = tileBoundsMinX + _tileSize - 1;
                var tileBoundsMaxY = tileBoundsMinY + _tileSize - 1;

                var tileCompletelyInside =
                    tileBoundsMinX >= minX && tileBoundsMaxX <= maxX &&
                    tileBoundsMinY >= minY && tileBoundsMaxY <= maxY;

                if (tileCompletelyInside)
                {
                    // Fast path: add all entries from this tile
                    result.AddRange(tile.Values);
                }
                else
                {
                    // Boundary tile: filter entries
                    foreach (var entry in tile.Values)
                    {
                        if (entry.X >= minX && entry.X <= maxX &&
                            entry.Y >= minY && entry.Y <= maxY)
                        {
                            result.Add(entry);
                        }
                    }
                }
            }
        }

        return result.ToArray();
    }

    public Entry[] GetWithinRadius(int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative");

        var radiusSquared = (long)radius * radius;
        var result = new List<Entry>();

        // Calculate bounding box for the circle
        var minX = Math.Max(0, -radius);
        var minY = Math.Max(0, -radius);
        var maxX = Math.Min(_maxCoordinate - 1, radius);
        var maxY = Math.Min(_maxCoordinate - 1, radius);

        // Calculate tile range
        var minTileX = minX >> _tileShift;
        var minTileY = minY >> _tileShift;
        var maxTileX = maxX >> _tileShift;
        var maxTileY = maxY >> _tileShift;

        // Iterate only over existing tiles (sparse iteration)
        foreach (var (tileKey, tile) in _tiles)
        {
            var (tileX, tileY) = tileKey;

            // Skip tiles outside the bounding box
            if (tileX < minTileX || tileX > maxTileX || tileY < minTileY || tileY > maxTileY)
                continue;

            // Calculate tile bounds
            var tileBoundsMinX = tileX << _tileShift;
            var tileBoundsMinY = tileY << _tileShift;
            var tileBoundsMaxX = tileBoundsMinX + _tileSize - 1;
            var tileBoundsMaxY = tileBoundsMinY + _tileSize - 1;

            // Check if tile is completely inside the circle
            // (all four corners are inside the circle)
            var allCornersInside = true;
            var corners = new int[][]
            {
                [tileBoundsMinX, tileBoundsMinY],
                [tileBoundsMaxX, tileBoundsMinY],
                [tileBoundsMinX, tileBoundsMaxY],
                [tileBoundsMaxX, tileBoundsMaxY]
            };

            foreach (var corner in corners)
            {
                var distSq = (long)corner[0] * corner[0] + (long)corner[1] * corner[1];
                if (distSq > radiusSquared)
                {
                    allCornersInside = false;
                    break;
                }
            }

            if (allCornersInside)
            {
                // Fast path: add all entries from this tile
                result.AddRange(tile.Values);
            }
            else
            {
                // Boundary tile: filter entries
                foreach (var entry in tile.Values)
                {
                    if ((long)entry.X * entry.X + (long)entry.Y * entry.Y <= radiusSquared)
                    {
                        result.Add(entry);
                    }
                }
            }
        }

        return result.ToArray();
    }

    public Entry[] GetWithinRadius(int centerX, int centerY, int radius)
    {
        ValidateCoordinates(centerX, centerY);
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative");

        var radiusSquared = (long)radius * radius;
        var result = new List<Entry>();

        // Calculate bounding box for the circle
        var minX = Math.Max(0, centerX - radius);
        var minY = Math.Max(0, centerY - radius);
        var maxX = Math.Min(_maxCoordinate - 1, centerX + radius);
        var maxY = Math.Min(_maxCoordinate - 1, centerY + radius);

        // Calculate tile range
        var minTileX = minX >> _tileShift;
        var minTileY = minY >> _tileShift;
        var maxTileX = maxX >> _tileShift;
        var maxTileY = maxY >> _tileShift;

        // Iterate only over existing tiles (sparse iteration)
        foreach (var (tileKey, tile) in _tiles)
        {
            var (tileX, tileY) = tileKey;

            // Skip tiles outside the bounding box
            if (tileX < minTileX || tileX > maxTileX || tileY < minTileY || tileY > maxTileY)
                continue;

            // Calculate tile bounds
            var tileBoundsMinX = tileX << _tileShift;
            var tileBoundsMinY = tileY << _tileShift;
            var tileBoundsMaxX = tileBoundsMinX + _tileSize - 1;
            var tileBoundsMaxY = tileBoundsMinY + _tileSize - 1;

            // Check if tile is completely inside the circle
            // (all four corners are inside the circle)
            var allCornersInside = true;
            var corners = new int[][]
            {
                [tileBoundsMinX, tileBoundsMinY],
                [tileBoundsMaxX, tileBoundsMinY],
                [tileBoundsMinX, tileBoundsMaxY],
                [tileBoundsMaxX, tileBoundsMaxY]
            };

            foreach (var corner in corners)
            {
                long dx = corner[0] - centerX;
                long dy = corner[1] - centerY;
                var distSq = dx * dx + dy * dy;
                if (distSq > radiusSquared)
                {
                    allCornersInside = false;
                    break;
                }
            }

            if (allCornersInside)
            {
                // Fast path: add all entries from this tile
                result.AddRange(tile.Values);
            }
            else
            {
                // Boundary tile: filter entries
                foreach (var entry in tile.Values)
                {
                    long dx = entry.X - centerX;
                    long dy = entry.Y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        result.Add(entry);
                    }
                }
            }
        }

        return result.ToArray();
    }

    public void Clear()
    {
        _tiles.Clear();
        _count = 0;
    }

    public int Count => _count;

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {_maxCoordinate - 1}");
        if (y < 0 || y >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {_maxCoordinate - 1}");
    }
}

