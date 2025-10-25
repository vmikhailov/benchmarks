namespace TreeMap;

/// <summary>
/// SortedDictionary-based storage for labels on a sparse 2D map.
/// Uses x^2 + y^2 as the key with a balanced tree structure.
///
/// Data Structure: SortedDictionary where key = x^2 + y^2, value = List of Entry
/// Time Complexity:
///   - Add: O(log n) for tree operation + O(k) for collision list
///   - Get: O(log n) for tree lookup + O(k) for collision list scan
///   - Remove: O(log n) for tree operation + O(k) for collision list
///   - GetWithinRadius: O(log n + m) where m = entries with key less than R^2
///   - List: O(n) where n = number of labels
/// Space Complexity: O(n) where n = number of labels
///
/// Advantages:
/// - Balanced tree (Red-Black tree internally) - no worst-case degradation like BST
/// - Direct key-value mapping (simpler than SortedSet)
/// - Built-in ordering by distance from origin
/// </summary>
public class MapStorage_SortedDictionary : IMapStorage
{
    private class Entry
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; }

        public Entry(int x, int y, string label)
        {
            X = x;
            Y = y;
            Label = label;
        }
    }

    private readonly SortedDictionary<long, List<Entry>> _sortedDict;
    private readonly int _maxCoordinate;
    private int _count;

    /// <summary>
    /// Initializes a new SortedDictionary-based map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    public MapStorage_SortedDictionary(int maxCoordinate = 1_000_000)
    {
        _sortedDict = new SortedDictionary<long, List<Entry>>();
        _maxCoordinate = maxCoordinate;
        _count = 0;
    }

    /// <summary>
    /// Computes the key for given coordinates.
    /// </summary>
    private long ComputeKey(int x, int y)
    {
        return (long)x * x + (long)y * y;
    }

    public bool Add(int x, int y, string label)
    {
        ValidateCoordinates(x, y);
        ArgumentNullException.ThrowIfNull(label);

        var key = ComputeKey(x, y);

        // Try to find an existing list with this key
        if (_sortedDict.TryGetValue(key, out var entries))
        {
            // Check if coordinates already exist
            foreach (var entry in entries)
            {
                if (entry.X == x && entry.Y == y)
                {
                    // Update existing entry
                    entry.Label = label;
                    return false;
                }
            }

            // Add a new entry to collision list
            entries.Add(new Entry(x, y, label));
            _count++;
            return true;
        }
        else
        {
            // Create a new list for this key
            var newList = new List<Entry> { new Entry(x, y, label) };
            _sortedDict[key] = newList;
            _count++;
            return true;
        }
    }

    public string? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);

        if (_sortedDict.TryGetValue(key, out var entries))
        {
            // Search through collision list
            foreach (var entry in entries)
            {
                if (entry.X == x && entry.Y == y)
                    return entry.Label;
            }
        }

        return null;
    }

    public bool TryGet(int x, int y, out string? label)
    {
        label = Get(x, y);
        return label != null;
    }

    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);

        if (_sortedDict.TryGetValue(key, out var entries))
        {
            // Find and remove the specific entry
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].X == x && entries[i].Y == y)
                {
                    entries.RemoveAt(i);
                    _count--;

                    // If list is now empty, remove the key
                    if (entries.Count == 0)
                    {
                        _sortedDict.Remove(key);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public bool Contains(int x, int y)
    {
        return Get(x, y) != null;
    }

    public IEnumerable<(int x, int y, string label)> ListAll()
    {
        foreach (var kvp in _sortedDict)
        {
            foreach (var entry in kvp.Value)
            {
                yield return (entry.X, entry.Y, entry.Label);
            }
        }
    }

    public IEnumerable<(int x, int y, string label)> GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        // Calculate the minimum and maximum possible distances (x² + y²) for points in the region
        // Minimum distance: closest corner to origin
        var minDistSquared = (long)Math.Min(Math.Abs(minX), Math.Abs(maxX)) * Math.Min(Math.Abs(minX), Math.Abs(maxX)) +
                            (long)Math.Min(Math.Abs(minY), Math.Abs(maxY)) * Math.Min(Math.Abs(minY), Math.Abs(maxY));

        // If region contains origin, min distance is 0
        if (minX <= 0 && maxX >= 0 && minY <= 0 && maxY >= 0)
            minDistSquared = 0;
        else
        {
            // Find the closest point in the rectangle to the origin
            var closestX = Math.Max(minX, Math.Min(0, maxX));
            var closestY = Math.Max(minY, Math.Min(0, maxY));
            minDistSquared = (long)closestX * closestX + (long)closestY * closestY;
        }

        // Maximum distance: farthest corner from origin
        var maxDistSquared = Math.Max((long)minX * minX, (long)maxX * maxX) +
                            Math.Max((long)minY * minY, (long)maxY * maxY);

        // Use binary search approach: iterate through sorted keys in the valid range
        foreach (var kvp in _sortedDict)
        {
            // Skip entries with keys below minimum distance
            if (kvp.Key < minDistSquared)
                continue;

            // Stop when we exceed maximum possible distance
            if (kvp.Key > maxDistSquared)
                break;

            // Check each entry in the collision list
            foreach (var entry in kvp.Value)
            {
                if (entry.X >= minX && entry.X <= maxX &&
                    entry.Y >= minY && entry.Y <= maxY)
                {
                    yield return (entry.X, entry.Y, entry.Label);
                }
            }
        }
    }

    public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative");

        var radiusSquared = (long)radius * radius;

        // Iterate through sorted dictionary - keys are ordered by x^2 + y^2
        // Stop as soon as we reach a key >= radiusSquared
        foreach (var kvp in _sortedDict)
        {
            if (kvp.Key >= radiusSquared)
                break; // All remaining keys will be larger

            // All entries in this list are within radius
            foreach (var entry in kvp.Value)
            {
                yield return (entry.X, entry.Y, entry.Label);
            }
        }
    }

    public void Clear()
    {
        _sortedDict.Clear();
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

    /// <summary>
   /// Gets statistics about the SortedDictionary structure and collisions.
    /// </summary>
    public (int nodes, int maxCollisions, int totalCollisions) GetStatistics()
    {
        var nodes = _sortedDict.Count;
        var maxCollisions = 0;
        var totalCollisions = 0;

        foreach (var kvp in _sortedDict)
        {
            var collisionCount = kvp.Value.Count;
            if (collisionCount > 1)
            {
                totalCollisions += collisionCount - 1;
                maxCollisions = Math.Max(maxCollisions, collisionCount);
            }
        }

        return (nodes, maxCollisions, totalCollisions);
    }
}

