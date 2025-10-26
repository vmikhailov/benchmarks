namespace TreeMap;

/// <summary>
/// Storage implementation using string keys in format "x,y".
/// This is a simple implementation to compare performance against tuple-keyed dictionary.
///
/// Data Structure: Dictionary with string composite key "x,y"
/// Time Complexity:
///   - Add: O(1) average, O(n) worst case
///   - Get: O(1) average, O(n) worst case
///   - Remove: O(1) average, O(n) worst case
///   - List: O(n) where n = number of labels
/// Space Complexity: O(n) where n = number of labels (plus string overhead)
/// </summary>
public class MapStorage_StringKey : IMapStorage
{
    private readonly Dictionary<string, Entry> _labels;
    private readonly int _maxCoordinate;

    /// <summary>
    /// Initializes a new map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    public MapStorage_StringKey(int maxCoordinate)
    {
        _labels = new();
        _maxCoordinate = maxCoordinate;
    }

    public MapStorage_StringKey() : this(1_000_000)
    {
    }

    private static string CreateKey(int x, int y) => $"{x},{y}";

    /// <summary>
    /// Adds or updates a label at the specified coordinates.
    /// </summary>
    /// <param name="entry">The entry to add</param>
    /// <returns>True if added, false if updated existing</returns>
    /// <exception cref="ArgumentOutOfRangeException">If coordinates are invalid</exception>
    public bool Add(Entry entry)
    {
        ValidateCoordinates(entry.X, entry.Y);

        var key = CreateKey(entry.X, entry.Y);
        var isNew = !_labels.ContainsKey(key);
        _labels[key] = entry;
        return isNew;
    }

    /// <summary>
    /// Retrieves a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Entry if found, null otherwise</returns>
    public Entry? Get(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _labels.GetValueOrDefault(CreateKey(x, y));
    }

    /// <summary>
    /// Tries to retrieve a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="entry">The entry if found</param>
    /// <returns>True if entry exists, false otherwise</returns>
    public bool TryGet(int x, int y, out Entry? entry)
    {
        ValidateCoordinates(x, y);
        return _labels.TryGetValue(CreateKey(x, y), out entry);
    }

    /// <summary>
    /// Removes a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if removed, false if not found</returns>
    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _labels.Remove(CreateKey(x, y));
    }

    /// <summary>
    /// Checks if a label exists at the specified coordinates.
    /// </summary>
    public bool Contains(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _labels.ContainsKey(CreateKey(x, y));
    }

    /// <summary>
    /// Returns all labels with their coordinates.
    /// </summary>
    /// <returns>Array of entries</returns>
    public Entry[] ListAll()
    {
        var result = new Entry[_labels.Count];
        var index = 0;
        foreach (var entry in _labels.Values)
        {
            result[index++] = entry;
        }
        return result;
    }

    /// <summary>
    /// Returns labels within a rectangular region.
    /// </summary>
    /// <param name="minX">Minimum X coordinate (inclusive)</param>
    /// <param name="minY">Minimum Y coordinate (inclusive)</param>
    /// <param name="maxX">Maximum X coordinate (inclusive)</param>
    /// <param name="maxY">Maximum Y coordinate (inclusive)</param>
    /// <returns>Array of entries within the specified region</returns>
    public Entry[] GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        var result = new List<Entry>();
        foreach (var entry in _labels.Values)
        {
            if (entry.X >= minX && entry.X <= maxX &&
                entry.Y >= minY && entry.Y <= maxY)
            {
                result.Add(entry);
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

        foreach (var entry in _labels.Values)
        {
            if ((long)entry.X * entry.X + (long)entry.Y * entry.Y < radiusSquared)
            {
                result.Add(entry);
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

        foreach (var entry in _labels.Values)
        {
            long dx = entry.X - centerX;
            long dy = entry.Y - centerY;
            if (dx * dx + dy * dy <= radiusSquared)
            {
                result.Add(entry);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Clears all labels from the map.
    /// </summary>
    public void Clear()
    {
        _labels.Clear();
    }

    /// <summary>
    /// Gets the total number of labels stored.
    /// </summary>
    public int Count => _labels.Count;

    private void ValidateCoordinates(int x, int y)
    {
        if (x < 0 || x >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {_maxCoordinate - 1}");
        if (y < 0 || y >= _maxCoordinate)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {_maxCoordinate - 1}");
    }
}

