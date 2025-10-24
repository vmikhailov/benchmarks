# MapStorage_SortedArray Implementation

## Overview
A new implementation using a **sorted array** ordered by x² + y² as the storage mechanism for 2D map labels.

## Data Structure
- **Internal Storage**: `List<Entry>` where each entry contains: key (x² + y²), x, y, label
- **Ordering**: Always sorted by key in ascending order
- **Collision Handling**: Multiple entries can have the same key (stored consecutively)

## Key Features

### 1. Binary Search for Efficient Lookups
```csharp
private int BinarySearchForKey(long key)
{
    // Standard binary search returning index or ~insertionPoint
}
```

### 2. Optimized Radius Queries (Main Advantage!)
```csharp
public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
{
    long radiusSquared = (long)radius * radius;
    int upperBoundIndex = BinarySearchUpperBound(radiusSquared);
    
    // Simply return all entries before the upper bound!
    for (int i = 0; i < upperBoundIndex; i++)
        yield return (_entries[i].X, _entries[i].Y, _entries[i].Label);
}
```

Since the array is sorted by x² + y², finding all points within radius R is:
1. Binary search for the first entry ≥ R² → O(log n)
2. Return all entries before that point → O(k) where k = results

**Total: O(log n + k) - Same as BST but with better constants due to array locality!**

## Time Complexity Comparison

| Operation | Dictionary | BST | SortedArray |
|-----------|-----------|-----|-------------|
| Add | O(1) avg | O(log n) avg | **O(n)** worst |
| Get | O(1) avg | O(log n) avg | O(log n) avg |
| Remove | O(1) avg | O(log n) avg | **O(n)** worst |
| GetWithinRadius | O(n) | O(log n + k) | **O(log n + k)** |
| ListAll | O(n) | O(n) | O(n) |

## Performance Characteristics

### Advantages ✅
1. **Excellent for GetWithinRadius**: Same algorithmic complexity as BST but better cache locality
2. **Cache-friendly**: Contiguous memory, better CPU cache utilization
3. **Simple implementation**: No complex tree balancing logic
4. **Predictable performance**: No worst-case tree imbalance issues

### Disadvantages ❌
1. **Slow insertions**: O(n) due to array shifting when inserting in middle
2. **Slow deletions**: O(n) due to array shifting when removing from middle
3. **Not suitable for write-heavy workloads**

## When to Use

### ✅ Best for:
- **Read-heavy workloads** with frequent radius queries
- Batch loading data once, then querying many times
- Scenarios where GetWithinRadius is the primary operation
- Small to medium datasets (100-10000 labels)

### ❌ Avoid when:
- Frequent insertions/deletions
- Write-heavy workloads
- Very large datasets with constant modifications

## Benchmark Results Expected

For **GetWithinRadius** with small radius (high selectivity):
- **SortedArray**: Should match or beat BST due to cache locality
- **Dictionary**: Will be slowest (must check all entries)

For **Add operations**:
- **Dictionary**: Fastest (O(1))
- **BST**: Medium (O(log n))
- **SortedArray**: Slowest (O(n))

## Code Example

```csharp
var storage = new MapStorage_SortedArray();

// Batch load data (acceptable performance)
storage.Add(10, 20, "close");
storage.Add(100, 200, "medium");
storage.Add(1000, 2000, "far");

// Efficient radius query (this is where it shines!)
var nearOrigin = storage.GetWithinRadius(150);
// Binary search finds upper bound in O(log n), then returns results

// Check statistics
var stats = storage.GetStatistics();
Console.WriteLine($"Total: {stats.totalEntries}, Unique keys: {stats.uniqueKeys}");
```

## Implementation Highlights

### Binary Search Upper Bound
The `BinarySearchUpperBound` method finds the first index where key ≥ target:
```csharp
private int BinarySearchUpperBound(long target)
{
    int left = 0, right = _entries.Count;
    while (left < right) {
        int mid = left + (right - left) / 2;
        if (_entries[mid].Key < target)
            left = mid + 1;
        else
            right = mid;
    }
    return left;
}
```

This is crucial for efficient range queries - we can find all entries < R² in O(log n).

## Summary

The **SortedArray** implementation fills a specific niche:
- Not as versatile as Dictionary
- Better cache locality than BST
- **Perfect for radius queries on relatively static datasets**

Combined with the other implementations, you now have three strategies:
1. **Dictionary**: General purpose, best overall
2. **BST**: Educational, shows collision handling
3. **SortedArray**: Specialized for radius queries, read-heavy workloads

