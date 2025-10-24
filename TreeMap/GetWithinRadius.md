# GetWithinRadius Feature

## Overview
Added a new method `GetWithinRadius(int radius)` to retrieve all labels within a circular distance R from the origin (0,0).

## Mathematical Definition
Returns all labels where: **x² + y² < R²**

This creates a circular query region centered at the origin.

## Implementation Details

### Dictionary Implementation
```csharp
public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
{
    long radiusSquared = (long)radius * radius;
    return _labels
        .Where(kvp => (long)kvp.Key.x * kvp.Key.x + (long)kvp.Key.y * kvp.Key.y < radiusSquared)
        .Select(kvp => (kvp.Key.x, kvp.Key.y, kvp.Value));
}
```
- **Time Complexity**: O(n) - Must check all labels
- **Approach**: Linear scan through dictionary, filtering by distance formula

### BST Implementation (Optimized!)
```csharp
public IEnumerable<(int x, int y, string label)> GetWithinRadius(int radius)
{
    long radiusSquared = (long)radius * radius;
    var result = new List<(int x, int y, string label)>();
    GetWithinRadiusRecursive(_root, radiusSquared, result);
    return result;
}

private void GetWithinRadiusRecursive(BSTNode? node, long radiusSquared, List<...> result)
{
    if (node == null) return;
    
    GetWithinRadiusRecursive(node.Left, radiusSquared, result);
    
    if (node.Key < radiusSquared)
    {
        result.AddRange(node.Entries);
        GetWithinRadiusRecursive(node.Right, radiusSquared, result);
    }
    // Skip right subtree if node.Key >= radiusSquared (optimization!)
}
```
- **Time Complexity**: O(k + log n) where k = matching labels
- **Key Advantage**: Since BST key = x² + y², we can prune the search tree
- **Optimization**: Skips entire right subtrees when key >= radiusSquared

## Performance Characteristics

### BST Advantage for Radius Queries
The BST implementation has a **unique advantage** for this specific query:

1. **Natural Key Ordering**: The BST is already ordered by x² + y²
2. **Efficient Pruning**: Can skip large portions of the tree
3. **Better Than Linear**: Only visits relevant nodes

### When BST Outperforms Dictionary
- Large datasets with most points far from origin
- Queries with small radius (high selectivity)
- Example: 1000 labels, radius captures 10% → BST visits ~100 nodes vs Dictionary checks all 1000

## Usage Example
```csharp
var storage = new MapStorage_Dictionary();
storage.Add(10, 20, "close");
storage.Add(1000, 2000, "far");

// Get all labels within 100 units from origin
var nearOrigin = storage.GetWithinRadius(100);
// Returns: (10, 20, "close") since 10² + 20² = 500 < 10000
```

## Benchmark Categories
The new method is benchmarked in the `GetWithinRadius` category with:
- LabelCount: 100, 1000
- Radius: 500000 (captures ~50% of randomly distributed points)
- Compares Dictionary vs BST performance

