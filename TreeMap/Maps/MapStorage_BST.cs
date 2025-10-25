namespace TreeMap;

/// <summary>
/// BST-based storage for labels on a sparse 2D map.
/// Uses x*x + y*y as the key, with collision handling via lists.
///
/// Data Structure: Binary Search Tree with collision lists
/// Time Complexity:
///   - Add: O(log n) average, O(n) worst case (unbalanced tree)
///   - Get: O(log n) average, O(n) worst case + O(k) for collision list
///   - Remove: O(log n) average, O(n) worst case + O(k) for collision list
///   - List: O(n) where n = number of labels
/// Space Complexity: O(n) where n = number of labels
///
/// Note: x*x + y*y creates collisions (e.g., (3,4) and (4,3) both map to 25)
/// Collisions are handled by storing multiple entries per BST node.
/// </summary>
public class MapStorage_BST : IMapStorage
{
    private class BSTNode
    {
        public long Key { get; set; }
        public List<Entry> Entries { get; set; }
        public BSTNode? Left { get; set; }
        public BSTNode? Right { get; set; }

        public BSTNode(long key)
        {
            Key = key;
            Entries = new();
        }
    }

    private BSTNode? _root;
    private int _count;
    private readonly int _maxCoordinate;

    /// <summary>
    /// Initializes a new BST-based map storage.
    /// </summary>
    /// <param name="maxCoordinate">Maximum valid coordinate (default: 1,000,000)</param>
    public MapStorage_BST(int maxCoordinate)
    {
        _root = null;
        _count = 0;
        _maxCoordinate = maxCoordinate;
    }

    public MapStorage_BST() : this(1_000_000)
    {
    }

    /// <summary>
    /// Computes the BST key for given coordinates.
    /// Uses x*x + y*y which creates interesting collision patterns.
    /// </summary>
    private long ComputeKey(int x, int y)
    {
        return (long)x * x + (long)y * y;
    }

    public bool Add(Entry entry)
    {
        ValidateCoordinates(entry.X, entry.Y);

        var key = ComputeKey(entry.X, entry.Y);
        var isNew = true;

        if (_root == null)
        {
            _root = new(key);
            _root.Entries.Add(entry);
            _count++;
            return true;
        }

        _root = AddRecursive(_root, key, entry, ref isNew);

        if (isNew)
            _count++;

        return isNew;
    }

    private BSTNode AddRecursive(BSTNode node, long key, Entry entry, ref bool isNew)
    {
        if (node == null)
        {
            var newNode = new BSTNode(key);
            newNode.Entries.Add(entry);
            return newNode;
        }

        if (key < node.Key)
        {
            node.Left = AddRecursive(node.Left!, key, entry, ref isNew);
        }
        else if (key > node.Key)
        {
            node.Right = AddRecursive(node.Right!, key, entry, ref isNew);
        }
        else // key == node.Key (collision or update)
        {
            // Check if coordinates already exist
            for (var i = 0; i < node.Entries.Count; i++)
            {
                if (node.Entries[i].X == entry.X && node.Entries[i].Y == entry.Y)
                {
                    // Update existing entry
                    node.Entries[i] = entry;
                    isNew = false;
                    return node;
                }
            }

            // Add new entry (collision case)
            node.Entries.Add(entry);
        }

        return node;
    }

    public Entry? Get(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);
        var node = FindNode(_root, key);

        if (node == null)
            return null;

        // Search through collision list
        foreach (var entry in node.Entries)
        {
            if (entry.X == x && entry.Y == y)
                return entry;
        }

        return null;
    }

    public bool TryGet(int x, int y, out Entry? entry)
    {
        entry = Get(x, y);
        return entry != null;
    }

    private BSTNode? FindNode(BSTNode? node, long key)
    {
        if (node == null)
            return null;

        if (key < node.Key)
            return FindNode(node.Left, key);
        else if (key > node.Key)
            return FindNode(node.Right, key);
        else
            return node;
    }

    public bool Remove(int x, int y)
    {
        ValidateCoordinates(x, y);

        var key = ComputeKey(x, y);
        var removed = false;
        _root = RemoveRecursive(_root, key, x, y, ref removed);

        if (removed)
            _count--;

        return removed;
    }

    private BSTNode? RemoveRecursive(BSTNode? node, long key, int x, int y, ref bool removed)
    {
        if (node == null)
            return null;

        if (key < node.Key)
        {
            node.Left = RemoveRecursive(node.Left, key, x, y, ref removed);
            return node;
        }
        else if (key > node.Key)
        {
            node.Right = RemoveRecursive(node.Right, key, x, y, ref removed);
            return node;
        }
        else // key == node.Key
        {
            // Find and remove the specific entry
            for (var i = 0; i < node.Entries.Count; i++)
            {
                if (node.Entries[i].X == x && node.Entries[i].Y == y)
                {
                    node.Entries.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            // If node still has entries (collision case), keep the node
            if (node.Entries.Count > 0)
                return node;

            // Node is now empty, remove it from tree
            // Case 1: No children
            if (node.Left == null && node.Right == null)
                return null;

            // Case 2: One child
            if (node.Left == null)
                return node.Right;
            if (node.Right == null)
                return node.Left;

            // Case 3: Two children - find inorder successor (leftmost node in right subtree)
            var successor = FindMin(node.Right);
            node.Key = successor.Key;
            node.Entries = new(successor.Entries);
            node.Right = RemoveMin(node.Right);

            return node;
        }
    }

    private BSTNode FindMin(BSTNode node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }

    private BSTNode? RemoveMin(BSTNode? node)
    {
        if (node == null)
            return null;
        if (node.Left == null)
            return node.Right;
        node.Left = RemoveMin(node.Left);
        return node;
    }

    public bool Contains(int x, int y)
    {
        return Get(x, y) != null;
    }

    public Entry[] ListAll()
    {
        var result = new List<Entry>();
        InOrderTraversal(_root, result);
        return result.ToArray();
    }

    private void InOrderTraversal(BSTNode? node, List<Entry> result)
    {
        if (node == null)
            return;

        InOrderTraversal(node.Left, result);
        result.AddRange(node.Entries);
        InOrderTraversal(node.Right, result);
    }

    public Entry[] GetInRegion(int minX, int minY, int maxX, int maxY)
    {
        ValidateCoordinates(minX, minY);
        ValidateCoordinates(maxX, maxY);

        if (minX > maxX || minY > maxY)
            throw new ArgumentException("Invalid region bounds");

        var result = new List<Entry>();
        var allEntries = ListAll();

        foreach (var entry in allEntries)
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

        // Optimized: Only traverse BST nodes with key < radiusSquared
        GetWithinRadiusRecursive(_root, radiusSquared, result);

        return result.ToArray();
    }

    private void GetWithinRadiusRecursive(BSTNode? node, long radiusSquared, List<Entry> result)
    {
        if (node == null)
            return;

        // Only traverse left subtree (smaller keys)
        GetWithinRadiusRecursive(node.Left, radiusSquared, result);

        // Check if current node's key is within radius
        if (node.Key < radiusSquared)
        {
            // Add all entries from this node
            result.AddRange(node.Entries);

            // Continue to right subtree (might have keys < radiusSquared)
            GetWithinRadiusRecursive(node.Right, radiusSquared, result);
        }
        // If node.Key >= radiusSquared, don't traverse right (all keys will be too large)
    }

    public void Clear()
    {
        _root = null;
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
    /// Gets statistics about the BST structure and collisions.
    /// Useful for understanding the data distribution.
    /// </summary>
    public (int nodes, int maxCollisions, int totalCollisions, int treeHeight) GetStatistics()
    {
        var nodes = 0;
        var maxCollisions = 0;
        var totalCollisions = 0;
        var height = GetHeight(_root);

        CountNodes(_root, ref nodes, ref maxCollisions, ref totalCollisions);

        return (nodes, maxCollisions, totalCollisions, height);
    }

    private void CountNodes(BSTNode? node, ref int nodes, ref int maxCollisions, ref int totalCollisions)
    {
        if (node == null)
            return;

        nodes++;
        var collisionCount = node.Entries.Count;
        if (collisionCount > 1)
        {
            totalCollisions += collisionCount - 1;
            maxCollisions = Math.Max(maxCollisions, collisionCount);
        }

        CountNodes(node.Left, ref nodes, ref maxCollisions, ref totalCollisions);
        CountNodes(node.Right, ref nodes, ref maxCollisions, ref totalCollisions);
    }

    private int GetHeight(BSTNode? node)
    {
        if (node == null)
            return 0;
        return 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    }
}

