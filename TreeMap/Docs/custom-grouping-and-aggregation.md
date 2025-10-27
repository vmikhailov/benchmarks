# BenchmarkDotNet Custom Grouping and Aggregation

## Summary

**Custom Grouping Rules:** ❌ **NOT SUPPORTED** - `BenchmarkLogicalGroupRule` is an **enum**, not an extensible base class.

**Custom Aggregation:** ✅ **SUPPORTED** via custom columns and exporters.

## Built-in Grouping Rules (Enum Values)

```csharp
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]     // Group by benchmark method
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]     // Group by all parameter combinations
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]   // Group by [BenchmarkCategory] attribute
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByJob)]        // Group by job configuration
```

## Custom Aggregation Options

### 1. Custom Columns (✅ Recommended)

Create columns that aggregate results across parameter values:

```csharp
// See CustomColumns.cs for implementations:
- GeometricMeanAcrossDataSizesColumn - Computes geometric mean across all data sizes
- AverageAcrossDataSizesColumn - Computes average across all data sizes
- WeightedScoreColumn - Custom weighted average with configurable weights
```

**Usage:**
```csharp
[Config(typeof(MyConfig))]
public class MyBenchmark
{
    private class MyConfig : ManualConfig
    {
        public MyConfig()
        {
            AddColumn(new GeometricMeanAcrossDataSizesColumn());
            AddColumn(new AverageAcrossDataSizesColumn());
            
            var weights = new Dictionary<int, double>
            {
                { 1000, 0.5 },   // Small datasets: 50% weight
                { 10000, 0.3 },  // Medium: 30%
                { 50000, 0.2 }   // Large: 20%
            };
            AddColumn(new WeightedScoreColumn(weights));
        }
    }
}
```

### 2. Category-Based Pseudo-Grouping

Use `[BenchmarkCategory]` with `ByCategory` grouping:

```csharp
[BenchmarkCategory("Small")]
[Params(1000)]
public void Benchmark_Small() { ... }

[BenchmarkCategory("Medium")]
[Params(10000)]
public void Benchmark_Medium() { ... }

[BenchmarkCategory("Large")]
[Params(50000)]
public void Benchmark_Large() { ... }
```

### 3. Custom Exporters

For post-processing and custom report generation:

```csharp
public class AggregatedExporter : IExporter
{
    public void ExportToFiles(Summary summary, ILogger consoleLogger)
    {
        // Group results by any criteria
        var grouped = summary.Reports
            .GroupBy(r => r.BenchmarkCase.Parameters["Storage"]);
            
        foreach (var group in grouped)
        {
            var avg = group.Average(r => r.ResultStatistics.Mean);
            // Custom output logic
        }
    }
}
```

## Example: Comparing Storage Types Across Data Sizes

See `WeightedBenchmarkWithCustomAggregation.cs` for a complete example that:

1. Runs benchmarks with multiple parameter combinations (Storage type × NumberOfLabels)
2. Shows individual results for each combination
3. Adds custom columns showing:
   - Geometric mean across all data sizes (for each storage type)
   - Simple average across all data sizes
   - Weighted score using custom weights

The custom columns allow you to see both:
- Individual performance at each data size
- Aggregated metrics to compare overall performance

## What You CAN'T Do

❌ Create custom grouping rules by inheriting from `BenchmarkLogicalGroupRule`
❌ Extend the grouping logic to group by arbitrary parameter combinations
❌ Create hierarchical grouping (group by X within group by Y)

## What You CAN Do

✅ Use built-in grouping (ByMethod, ByParams, ByCategory, ByJob)
✅ Create custom columns that show aggregated metrics
✅ Use [BenchmarkCategory] for semantic grouping
✅ Post-process results with custom exporters
✅ Filter benchmarks to run specific subsets
✅ Use RankColumn to rank results within groups

## Recommendation

For your use case (comparing storage types across multiple data sizes):

1. Use `[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]` - groups all parameter combos under each benchmark method
2. Add custom aggregation columns (GeometricMean, Average, WeightedScore)
3. Use `[RankColumn]` to rank storage types
4. The result table will show:
   - Each individual run (Storage × DataSize combination)
   - Aggregated metrics for each storage type
   - Rankings to quickly identify the best performer

This gives you the best of both worlds: detailed per-size results AND aggregated comparisons.

