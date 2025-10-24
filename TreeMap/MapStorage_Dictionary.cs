namespace TreeMap;

/// <summary>
/// Efficient in-memory storage for labels on a sparse 2D map.
/// Optimized for ~100-1000 labels on a 1,000,000 x 1,000,000 map.
///
/// Data Structure: Dictionary with (x,y) composite key
/// Time Complexity:
///   - Add: O(1) average, O(n) worst case
///   - Get: O(1) average, O(n) worst case
///   - Remove: O(1) average, O(n) worst case
///   - List: O(n) where n = number of labels
/// Space Complexity: O(n) where n = number of labels
/// </summary>
public class MapStorage_Dictionary : IMapStorage
{
    private readonly Dictionary<(int x, int y), string> _labels;
    private readonly int _maxCoordinate;

    /// <summary>
    /// Initializes a new map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    public MapStorage_Dictionary(int maxCoordinate = 1_000_000)
    {
        _labels = new();
        _maxCoordinate = maxCoordinate;
    }

    /// <summary>
    /// Adds or updates a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate (0 to maxCoordinate-1)</param>
    /// <param name="y">Y coordinate (0 to maxCoordinate-1)</param>
    /// <param name="label">Label text</param>
    /// <returns>True if added, false if updated existing</returns>
    /// <exception cref="ArgumentOutOfRangeException">If coordinates are invalid</exception>
    /// <exception cref="ArgumentNullException">If label is null</exception>
    public bool Add(int x, int y, string label)
    {
        ValidateCoordinates(x, y);
        if (label == null)
            throw new ArgumentNullException(nameof(label));

        var key = (x, y);
        var isNew = !_labels.ContainsKey(key);
        _labels[key] = label;
        return isNew;
    }

    /// <summary>
    /// Retrieves a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Label text if found, null otherwise</returns>
    public string? Get(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _labels.TryGetValue((x, y), out var label) ? label : null;
    }

    /// <summary>
    /// Tries to retrieve a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="label">The label if found</param>
    /// <returns>True if label exists, false otherwise</returns>
    public bool TryGet(int x, int y, out string? label)
    {
        ValidateCoordinates(x, y);
        return _labels.TryGetValue((x, y), out label);
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
        return _labels.Remove((x, y));
    }

    /// <summary>
    /// Checks if a label exists at the specified coordinates.
    /// </summary>
    public bool Contains(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _labels.ContainsKey((x, y));
    }

    /// <summary>
    /// Returns all labels with their coordinates.
    /// </summary>
    /// <returns>Enumerable of (x, y, label) tuples</returns>
    public IEnumerable<(int x, int y, string label)> ListAll()
    {
        return _labels.Select(kvp => (kvp.Key.x, kvp.Key.y, kvp.Value));
    }

    /// <summary>
    /// Returns labels within a rectangular region.
    /// </summary>
    /// <param name="minX">Minimum X coordinate (inclusive)</param>
    /// <param name="minY">Minimum Y coordinate (inclusive)</param>
    /// <param name="maxX">Maximum X coordinate (inclusive)</param>
    /// <param name="maxY">Maximum Y coordinate (inclusive)</param>
    /// <returns>Labels within the specified region</returns>
    public IEnumerable<(int x, int y, string label)> GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        return _labels
            .Where(kvp => kvp.Key.x >= minX && kvp.Key.x <= maxX &&
                         kvp.Key.y >= minY && kvp.Key.y <= maxY)
            .Select(kvp => (kvp.Key.x, kvp.Key.y, kvp.Value));
    }

    public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative");

        var radiusSquared = (long)radius * radius;

        return _labels
            .Where(kvp => (long)kvp.Key.x * kvp.Key.x + (long)kvp.Key.y * kvp.Key.y < radiusSquared)
            .Select(kvp => (kvp.Key.x, kvp.Key.y, kvp.Value));
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
