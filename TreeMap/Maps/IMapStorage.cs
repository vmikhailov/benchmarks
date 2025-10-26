namespace TreeMap;

/// <summary>
/// Interface for storing and retrieving labels on a 2D map.
/// </summary>
public interface IMapStorage
{
    /// <summary>
    /// Adds or updates a label at the specified coordinates.
    /// </summary>
    /// <param name="entry">The entry to add</param>
    /// <returns>True if added, false if updated existing</returns>
    bool Add(Entry entry);

    /// <summary>
    /// Retrieves a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Entry if found, null otherwise</returns>
    Entry? Get(int x, int y);

    /// <summary>
    /// Tries to retrieve a label at the specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="entry">The entry if found</param>
    /// <returns>True if entry exists, false otherwise</returns>
    bool TryGet(int x, int y, out Entry? entry);

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
    /// <returns>Array of entries</returns>
    Entry[] ListAll();

    /// <summary>
    /// Returns labels within a rectangular region.
    /// </summary>
    /// <param name="minX">Minimum X coordinate (inclusive)</param>
    /// <param name="minY">Minimum Y coordinate (inclusive)</param>
    /// <param name="maxX">Maximum X coordinate (inclusive)</param>
    /// <param name="maxY">Maximum Y coordinate (inclusive)</param>
    /// <returns>Array of entries within the specified region</returns>
    Entry[] GetInRegion(int minX, int minY, int maxX, int maxY);

    /// <summary>
    /// Returns all labels within a circular distance R from origin (0,0).
    /// Returns labels where x² + y² &lt; R².
    /// </summary>
    /// <param name="radius">The radius from origin</param>
    /// <returns>Array of entries within the circular region</returns>
    Entry[] GetWithinRadius(int radius);

    /// <summary>
    /// Returns all labels within a circular distance R from a center point (centerX, centerY).
    /// Returns labels where (x - centerX)² + (y - centerY)² &lt;= R².
    /// </summary>
    /// <param name="centerX">X coordinate of the center point</param>
    /// <param name="centerY">Y coordinate of the center point</param>
    /// <param name="radius">The radius from the center point</param>
    /// <returns>Array of entries within the circular region</returns>
    Entry[] GetWithinRadius(int centerX, int centerY, int radius);

    /// <summary>
    /// Clears all labels from the map.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the total number of labels stored.
    /// </summary>
    int Count { get; }
}

