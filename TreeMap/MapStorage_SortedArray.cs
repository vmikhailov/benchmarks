namespace TreeMap;

/// <summary>
/// Sorted array-based storage for labels on a sparse 2D map.
/// Uses x² + y² as the sort key for efficient range queries.
///
/// Data Structure: Sorted array with binary search
/// Time Complexity:
///   - Add: O(n) worst case (insertion requires shifting)
///   - Get: O(log n) for binary search + O(k) for collision scan
///   - Remove: O(n) worst case (deletion requires shifting)
///   - GetWithinRadius: O(log n + k) where k = matching labels (binary search + scan)
///   - List: O(n) where n = number of labels
/// Space Complexity: O(n) where n = number of labels
///
/// Advantages:
/// - Excellent for GetWithinRadius queries (binary search to find range)
/// - Cache-friendly (contiguous memory)
/// - Simple implementation
///
/// Disadvantages:
/// - Slow insertions/deletions (requires array shifting)
/// - Best for read-heavy workloads
/// </summary>
public class MapStorage_SortedArray : IMapStorage
{
    private struct Entry
    {
        public long Key { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; }

        public Entry(long key, int x, int y, string label)
        {
            Key = key;
            X = x;
            Y = y;
            Label = label;
        }
    }

    private readonly List<Entry> _entries;
    private readonly int _maxCoordinate;

    /// <summary>
    /// Initializes a new sorted-array-based map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    public MapStorage_SortedArray(int maxCoordinate = 1_000_000)
    {
        _entries = [];
        _maxCoordinate = maxCoordinate;
    }

    /// <summary>
    /// Computes the sort key for given coordinates.
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

        // Binary search to find the first occurrence or insertion point
        var startIndex = BinarySearchFirstOccurrence(key);

        if (startIndex >= 0)
        {
            // Key exists, check for exact coordinate match
            // Scan forward through all entries with this key
            for (var i = startIndex; i < _entries.Count && _entries[i].Key == key; i++)
            {
                if (_entries[i].X == x && _entries[i].Y == y)
                {
                    // Update existing entry
                    _entries[i] = new(key, x, y, label);
                    return false;
                }
            }

            // Add new entry after existing ones with the same key
            var insertIndex = startIndex;
            while (insertIndex < _entries.Count && _entries[insertIndex].Key == key)
                insertIndex++;

            _entries.Insert(insertIndex, new(key, x, y, label));
            return true;
        }
        else
        {
            // Key doesn't exist, find insertion point using standard binary search
            var index = BinarySearchForKey(key);
            var insertIndex = ~index; // Bitwise complement gives insertion point
            _entries.Insert(insertIndex, new(key, x, y, label));
            return true;
        }
    }

    public string? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);
        var startIndex = BinarySearchFirstOccurrence(key);

        if (startIndex < 0)
            return null;

        // Search through all entries with this key
        for (var i = startIndex; i < _entries.Count && _entries[i].Key == key; i++)
        {
            if (_entries[i].X == x && _entries[i].Y == y)
                return _entries[i].Label;
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
        var startIndex = BinarySearchFirstOccurrence(key);

        if (startIndex < 0)
            return false;


        // Search for the exact coordinate match
        for (var i = startIndex; i < _entries.Count && _entries[i].Key == key; i++)
        {
            if (_entries[i].X == x && _entries[i].Y == y)
            {
                _entries.RemoveAt(i);
                return true;
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
        return _entries.Select(e => (e.X, e.Y, e.Label));
    }

    public IEnumerable<(int x, int y, string label)> GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        return _entries
            .Where(e => e.X >= minX && e.X <= maxX && e.Y >= minY && e.Y <= maxY)
            .Select(e => (e.X, e.Y, e.Label));
    }

    public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative");

        var radiusSquared = (long)radius * radius;

        // Binary search to find the first entry with the key >= radiusSquared
        var upperBoundIndex = BinarySearchUpperBound(radiusSquared);

        // Return all entries before the upper bound
        for (var i = 0; i < upperBoundIndex; i++)
        {
            yield return (_entries[i].X, _entries[i].Y, _entries[i].Label);
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public int Count => _entries.Count;

    /// <summary>
    /// Binary search to find any entry with the given key.
    /// Returns index if found, or ~index where the entry should be inserted.
    /// </summary>
    private int BinarySearchForKey(long key)
    {
        var left = 0;
        var right = _entries.Count - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var midKey = _entries[mid].Key;

            if (midKey == key)
                return mid;

            if (midKey < key)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return ~left; // Return bitwise complement of insertion point
    }

    /// <summary>
    /// Binary search to find the FIRST entry with the given key.
    /// Returns index if found, or -1 if not found.
    /// </summary>
    private int BinarySearchFirstOccurrence(long key)
    {
        var left = 0;
        var right = _entries.Count - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var midKey = _entries[mid].Key;

            if (midKey == key)
            {
                result = mid;
                right = mid - 1; // Continue searching in left half for first occurrence
            }
            else if (midKey < key)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return result;
    }

    /// <summary>
    /// Binary search to find the first index where key >= target.
    /// Used for efficient range queries.
    /// </summary>
    private int BinarySearchUpperBound(long target)
    {
        var left = 0;
        var right = _entries.Count;

        while (left < right)
        {
            var mid = left + (right - left) / 2;

            if (_entries[mid].Key < target)
                left = mid + 1;
            else
                right = mid;
        }

        return left;
    }

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {_maxCoordinate - 1}");
        if (y < 0 || y >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {_maxCoordinate - 1}");
    }

    /// <summary>
    /// Gets statistics about the sorted array structure.
    /// </summary>
    public (int totalEntries, int uniqueKeys, int maxCollisions) GetStatistics()
    {
        if (_entries.Count == 0)
            return (0, 0, 0);

        var uniqueKeys = 0;
        var maxCollisions = 0;
        var currentCollisions = 1;
        long? lastKey = null;

        foreach (var entry in _entries)
        {
            if (lastKey == null || entry.Key != lastKey)
            {
                if (lastKey != null)
                {
                    uniqueKeys++;
                    maxCollisions = Math.Max(maxCollisions, currentCollisions);
                }
                lastKey = entry.Key;
                currentCollisions = 1;
            }
            else
            {
                currentCollisions++;
            }
        }

        // Handle last group
        uniqueKeys++;
        maxCollisions = Math.Max(maxCollisions, currentCollisions);

        return (_entries.Count, uniqueKeys, maxCollisions);
    }
}

