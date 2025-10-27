namespace TreeMap;

/// <summary>
/// Efficient in-memory storage for labels on a sparse 2D map using dynamic adaptive tiling.
/// Tiles automatically split when they reach a capacity limit, splitting along the longest dimension.
/// Optimized for ~100-1000 labels on a 1,000,000 x 1,000,000 map.
///
/// Data Structure: Dictionary of variable-sized tiles that split when capacity is reached
/// Time Complexity:
///   - Add: O(1) average, O(log n) worst case when splitting
///   - Get: O(1) average
///   - Remove: O(1) average
///   - GetWithinRadius: O(t + m) where t = tiles checked, m = entries in those tiles
///   - GetInRegion: O(t + m) where t = tiles in region, m = entries in those tiles
/// Space Complexity: O(n) where n = number of labels
///
/// The dynamic tiling approach:
/// 1. Starts with a single large tile covering the entire map
/// 2. When a tile reaches capacity, it splits along its longest dimension
/// 3. Automatically adapts to data distribution
/// 4. Maintains efficiency even with clustered data
/// </summary>
public class MapStorage_DynamicTiled : IMapStorage
{
    private readonly Dictionary<int, Tile> _tiles;
    private readonly int _maxCoordinate;
    private readonly int _tileCapacity;
    private int _count;
    private int _nextTileId;

    /// <summary>
    /// Represents a rectangular tile that can contain entries.
    /// </summary>
    private class Tile
    {
        public int Id { get; }
        public int MinX { get; }
        public int MinY { get; }
        public int MaxX { get; }
        public int MaxY { get; }
        public Dictionary<(int x, int y), Entry> Entries { get; }

        public Tile(int id, int minX, int minY, int maxX, int maxY)
        {
            Id = id;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            Entries = new();
        }

        public bool Contains(int x, int y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
    }

    /// <summary>
    /// Initializes a new dynamic tiled map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    /// <param name="tileCapacity">Maximum entries per tile before splitting (default: 64)</param>
    public MapStorage_DynamicTiled(int maxCoordinate = 1_000_000, int tileCapacity = 64)
    {
        _tiles = new();
        _maxCoordinate = maxCoordinate;
        _tileCapacity = tileCapacity;
        _count = 0;
        _nextTileId = 0;

        // Start with a single tile covering the entire map
        var rootTile = new Tile(_nextTileId++, 0, 0, maxCoordinate - 1, maxCoordinate - 1);
        _tiles[rootTile.Id] = rootTile;
    }

    /// <summary>
    /// Finds the tile that contains the given coordinates.
    /// </summary>
    private Tile? FindTile(int x, int y)
    {
        foreach (var tile in _tiles.Values)
        {
            if (tile.Contains(x, y))
                return tile;
        }
        return null;
    }

    /// <summary>
    /// Splits a tile along its longest dimension when it reaches capacity.
    /// </summary>
    private void SplitTile(Tile tile)
    {
        var width = tile.Width;
        var height = tile.Height;

        // Can't split a 1x1 tile
        if (width == 1 && height == 1)
            return;

        // Split along the longest dimension
        Tile tile1, tile2;

        if (width >= height)
        {
            // Split horizontally (along X axis)
            var midX = tile.MinX + width / 2;
            tile1 = new(_nextTileId++, tile.MinX, tile.MinY, midX - 1, tile.MaxY);
            tile2 = new(_nextTileId++, midX, tile.MinY, tile.MaxX, tile.MaxY);
        }
        else
        {
            // Split vertically (along Y axis)
            var midY = tile.MinY + height / 2;
            tile1 = new(_nextTileId++, tile.MinX, tile.MinY, tile.MaxX, midY - 1);
            tile2 = new(_nextTileId++, tile.MinX, midY, tile.MaxX, tile.MaxY);
        }

        // Redistribute entries to new tiles
        foreach (var entry in tile.Entries.Values)
        {
            if (tile1.Contains(entry.X, entry.Y))
            {
                tile1.Entries[(entry.X, entry.Y)] = entry;
            }
            else if (tile2.Contains(entry.X, entry.Y))
            {
                tile2.Entries[(entry.X, entry.Y)] = entry;
            }
        }

        // Remove old tile and add new tiles
        _tiles.Remove(tile.Id);
        _tiles[tile1.Id] = tile1;
        _tiles[tile2.Id] = tile2;

        // Recursively split if new tiles are still over capacity
        if (tile1.Entries.Count > _tileCapacity)
            SplitTile(tile1);

        if (tile2.Entries.Count > _tileCapacity)
            SplitTile(tile2);
    }

    public bool Add(Entry entry)
    {
        ValidateCoordinates(entry.X, entry.Y);

        var tile = FindTile(entry.X, entry.Y);
        if (tile == null)
            throw new InvalidOperationException("No tile found for coordinates");

        var key = (entry.X, entry.Y);
        var isNew = !tile.Entries.ContainsKey(key);
        tile.Entries[key] = entry;

        if (isNew)
        {
            _count++;

            // Check if we need to split the tile
            if (tile.Entries.Count > _tileCapacity)
            {
                SplitTile(tile);
            }
        }

        return isNew;
    }

    public Entry? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var tile = FindTile(x, y);
        if (tile != null && tile.Entries.TryGetValue((x, y), out var entry))
        {
            return entry;
        }
        return null;
    }

    public bool TryGet(int x, int y, out Entry? entry)
    {
        ValidateCoordinates(x, y);

        var tile = FindTile(x, y);
        if (tile != null)
        {
            return tile.Entries.TryGetValue((x, y), out entry);
        }
        entry = null;
        return false;
    }

    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);

        var tile = FindTile(x, y);
        if (tile != null && tile.Entries.Remove((x, y)))
        {
            _count--;
            return true;
        }
        return false;
    }

    public bool Contains(int x, int y)
    {
        ValidateCoordinates(x, y);

        var tile = FindTile(x, y);
        return tile != null && tile.Entries.ContainsKey((x, y));
    }

    public Entry[] ListAll()
    {
        var result = new Entry[_count];
        var index = 0;

        foreach (var tile in _tiles.Values)
        {
            foreach (var entry in tile.Entries.Values)
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

        foreach (var tile in _tiles.Values)
        {
            // Skip tiles that don't intersect the region
            if (tile.MaxX < minX || tile.MinX > maxX || tile.MaxY < minY || tile.MinY > maxY)
                continue;

            // Check if tile is completely inside the region
            var tileCompletelyInside =
                tile.MinX >= minX && tile.MaxX <= maxX &&
                tile.MinY >= minY && tile.MaxY <= maxY;

            if (tileCompletelyInside)
            {
                // Fast path: add all entries from this tile
                result.AddRange(tile.Entries.Values);
            }
            else
            {
                // Boundary tile: filter entries
                foreach (var entry in tile.Entries.Values)
                {
                    if (entry.X >= minX && entry.X <= maxX &&
                        entry.Y >= minY && entry.Y <= maxY)
                    {
                        result.Add(entry);
                    }
                }
            }
        }

        return result.ToArray();
    }

    public Entry[] GetWithinRadius(int radius)
    {
        return GetWithinRadius(0, 0, radius);
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

        foreach (var tile in _tiles.Values)
        {
            // Skip tiles outside the bounding box
            if (tile.MaxX < minX || tile.MinX > maxX || tile.MaxY < minY || tile.MinY > maxY)
                continue;

            // Check if tile is completely inside the circle
            // (all four corners are inside the circle)
            var allCornersInside = true;
            var corners = new (int x, int y)[]
            {
                (tile.MinX, tile.MinY),
                (tile.MaxX, tile.MinY),
                (tile.MinX, tile.MaxY),
                (tile.MaxX, tile.MaxY)
            };

            foreach (var (cornerX, cornerY) in corners)
            {
                long dx = cornerX - centerX;
                long dy = cornerY - centerY;
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
                result.AddRange(tile.Entries.Values);
            }
            else
            {
                // Boundary tile: filter entries
                foreach (var entry in tile.Entries.Values)
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
        _nextTileId = 0;

        // Recreate root tile
        var rootTile = new Tile(_nextTileId++, 0, 0, _maxCoordinate - 1, _maxCoordinate - 1);
        _tiles[rootTile.Id] = rootTile;
    }

    public int Count => _count;

    /// <summary>
    /// Gets the current number of tiles in the storage.
    /// Useful for debugging and understanding the adaptive behavior.
    /// </summary>
    public int TileCount => _tiles.Count;

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {_maxCoordinate - 1}");
        if (y < 0 || y >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {_maxCoordinate - 1}");
    }
}

