namespace TreeMap;

/// <summary>
/// SortedDictionary-based storage for labels on a sparse 2D map.
/// Uses x^2 + y^2 as the key with a balanced tree structure.
///
/// Data Structure: SortedDictionary where key = x^2 + y^2, value = LinkedList of Entry
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
/// - LinkedList for O(1) node removal once node is found
/// - Built-in ordering by distance from origin
/// </summary>
public class MapStorage_SortedDictionary2 : IMapStorage
{
    private readonly SortedDictionary<long, LinkedList<Entry>> _sortedDict;
    private readonly int _maxCoordinate;
    private int _count;

    public MapStorage_SortedDictionary2(int maxCoordinate)
    {
        _sortedDict = new SortedDictionary<long, LinkedList<Entry>>();
        _maxCoordinate = maxCoordinate;
        _count = 0;
    }

    public MapStorage_SortedDictionary2() : this(1_000_000)
    {
    }

    private long ComputeKey(int x, int y)
    {
        return (long)x * x + (long)y * y;
    }

    public bool Add(Entry entry)
    {
        ValidateCoordinates(entry.X, entry.Y);

        var key = ComputeKey(entry.X, entry.Y);

        if (_sortedDict.TryGetValue(key, out var entries))
        {
            // Search for existing entry to update
            var node = entries.First;

            while (node != null)
            {
                if (node.Value.X == entry.X && node.Value.Y == entry.Y)
                {
                    node.Value = entry;
                    return false;
                }

                node = node.Next;
            }

            // Not found, add new entry
            entries.AddLast(entry);
            _count++;
            return true;
        }
        else
        {
            var newList = new LinkedList<Entry>();
            newList.AddLast(entry);
            _sortedDict[key] = newList;
            _count++;
            return true;
        }
    }

    public Entry? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);

        if (_sortedDict.TryGetValue(key, out var entries))
        {
            foreach (var entry in entries)
            {
                if (entry.X == x && entry.Y == y)
                    return entry;
            }
        }

        return null;
    }

    public bool TryGet(int x, int y, out Entry? entry)
    {
        entry = Get(x, y);
        return entry != null;
    }

    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);

        if (_sortedDict.TryGetValue(key, out var entries))
        {
            var node = entries.First;

            while (node != null)
            {
                if (node.Value.X == x && node.Value.Y == y)
                {
                    entries.Remove(node);
                    _count--;

                    if (entries.Count == 0)
                    {
                        _sortedDict.Remove(key);
                    }

                    return true;
                }

                node = node.Next;
            }
        }

        return false;
    }

    public bool Contains(int x, int y)
    {
        return Get(x, y) != null;
    }

    public Entry[] ListAll()
    {
        var result = new Entry[_count];
        var index = 0;

        foreach (var kvp in _sortedDict)
        {
            foreach (var entry in kvp.Value)
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

        long minDistSquared;

        if (minX <= 0 && maxX >= 0 && minY <= 0 && maxY >= 0)
            minDistSquared = 0;
        else
        {
            var closestX = Math.Max(minX, Math.Min(0, maxX));
            var closestY = Math.Max(minY, Math.Min(0, maxY));
            minDistSquared = (long)closestX * closestX + (long)closestY * closestY;
        }

        var maxDistSquared = Math.Max((long)minX * minX, (long)maxX * maxX) +
                             Math.Max((long)minY * minY, (long)maxY * maxY);

        var result = new List<Entry>();

        foreach (var kvp in _sortedDict)
        {
            if (kvp.Key < minDistSquared)
                continue;

            if (kvp.Key > maxDistSquared)
                break;

            foreach (var entry in kvp.Value)
            {
                if (entry.X >= minX && entry.X <= maxX &&
                    entry.Y >= minY && entry.Y <= maxY)
                {
                    result.Add(entry);
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

        foreach (var kvp in _sortedDict)
        {
            if (kvp.Key >= radiusSquared)
                break;

            foreach (var entry in kvp.Value)
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

        // Cannot optimize for arbitrary center points
        // Must check all entries
        foreach (var kvp in _sortedDict)
        {
            foreach (var entry in kvp.Value)
            {
                long dx = entry.X - centerX;
                long dy = entry.Y - centerY;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    result.Add(entry);
                }
            }
        }

        return result.ToArray();
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
