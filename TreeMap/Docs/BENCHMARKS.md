# Map Storage Benchmarking

This project includes comprehensive benchmarks comparing Dictionary and BST implementations for 2D map label storage.

## Running Benchmarks

To run the benchmarks:

```bash
dotnet run --configuration Release -- benchmark
```

**Important**: Always run benchmarks in Release mode for accurate performance measurements.

## Benchmark Suites

### 1. MapStorageBenchmarks
Compares Dictionary vs BST for common operations:
- **Add**: Inserting labels (100, 500, 1000 labels)
- **Get**: Retrieving labels by coordinates
- **ListAll**: Enumerating all labels
- **GetInRegion**: Spatial queries within a rectangle
- **Add/Remove**: Mixed operations

### 2. CollisionBenchmarks
Tests performance with collision-heavy data:
- Uses Pythagorean triples to generate coordinates with same x² + y² values
- Tests how well each implementation handles hash/key collisions
- Dictionary uses (x,y) tuple (no collisions)
- BST uses x² + y² key (intentional collisions)

## Benchmark Configuration

- **MemoryDiagnoser**: Shows memory allocations
- **RankColumn**: Ranks implementations by performance
- **Orderer**: Sorts results from fastest to slowest

## Expected Results

**Dictionary Implementation:**
- O(1) average case for Add/Get/Remove
- No collisions with composite key (x,y)
- Best overall performance

**BST Implementation:**
- O(log n) average case for balanced tree
- O(n) worst case if unbalanced
- Collision handling via lists
- Educational value showing data structures

## Output

BenchmarkDotNet generates detailed reports including:
- Mean execution time
- Memory allocations
- Standard deviation
- Comparison rankings
- Results saved in `BenchmarkDotNet.Artifacts/`

