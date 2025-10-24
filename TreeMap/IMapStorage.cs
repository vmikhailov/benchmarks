namespace TreeMap;

/// <summary>
/// Interface for storing and retrieving labels on a 2D map.
/// </summary>
public interface IMapStorage
{
    /// <summary>
    /// Adds or updates a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="label">Label text</param>
    /// <returns>True if added, false if updated existing</returns>
    bool Add(int x, int y, string label);

    /// <summary>
    /// Retrieves a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Label text if found, null otherwise</returns>
    string? Get(int x, int y);

    /// <summary>
    /// Tries to retrieve a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="label">The label if found</param>
    /// <returns>True if label exists, false otherwise</returns>
    bool TryGet(int x, int y, out string? label);

    /// <summary>
    /// Removes a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if removed, false if not found</returns>
    bool Remove(int x, int y);

    /// <summary>
    /// Checks if a label exists at the specified coordinates.
    /// </summary>
    bool Contains(int x, int y);

    /// <summary>
    /// Returns all labels with their coordinates.
    /// </summary>
    /// <returns>Enumerable of (x, y, label) tuples</returns>
    IEnumerable<(int x, int y, string label)> ListAll();

    /// <summary>
    /// Returns labels within a rectangular region.
    /// </summary>
    /// <param name="minX">Minimum X coordinate (inclusive)</param>
    /// <param name="minY">Minimum Y coordinate (inclusive)</param>
    /// <param name="maxX">Maximum X coordinate (inclusive)</param>
    /// <param name="maxY">Maximum Y coordinate (inclusive)</param>
    /// <returns>Labels within the specified region</returns>
    IEnumerable<(int x, int y, string label)> GetInRegion(int minX, int minY, int maxX, int maxY);

    /// <summary>
    /// Returns all labels within a circular distance R from origin (0,0).
    /// Returns labels where x² + y² &lt; R².
    /// </summary>
    /// <param name="radius">The radius from origin</param>
    /// <returns>Labels within the circular region</returns>
    IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius);

    /// <summary>
    /// Clears all labels from the map.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the total number of labels stored.
    /// </summary>
    int Count { get; }
}

