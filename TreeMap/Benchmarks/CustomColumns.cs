using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace TreeMap.Benchmarks;

/// <summary>
/// Custom column that computes the geometric mean of execution times
/// across all NumberOfLabels parameter values for each Storage type.
/// </summary>
public class GeometricMeanAcrossDataSizesColumn : IColumn
{
    public string Id => nameof(GeometricMeanAcrossDataSizesColumn);
    public string ColumnName => "GeoMean Time";
    public string Legend => "Geometric mean of execution time across all data sizes";
    public UnitType UnitType => UnitType.Time;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public bool IsAvailable(Summary summary) => true;
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        // Get the storage type for this benchmark case
        var storage = benchmarkCase.Parameters["Storage"];
        var method = benchmarkCase.Descriptor.WorkloadMethod.Name;

        // Find all reports with the same storage and method, but different NumberOfLabels
        var relatedReports = summary.Reports
            .Where(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == method &&
                       r.BenchmarkCase.Parameters["Storage"].ToString() == storage.ToString() &&
                       r.ResultStatistics != null)
            .ToList();

        if (relatedReports.Count == 0)
            return "N/A";

        // Calculate geometric mean
        var product = 1.0;
        foreach (var report in relatedReports)
        {
            if (report.ResultStatistics?.Mean != null)
                product *= report.ResultStatistics.Mean;
        }
        var geometricMean = Math.Pow(product, 1.0 / relatedReports.Count);

        return $"{geometricMean / 1_000_000:F2} ms";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        return GetValue(summary, benchmarkCase);
    }

    public override string ToString() => ColumnName;
}

/// <summary>
/// Custom column that shows the average execution time across all data sizes.
/// </summary>
public class AverageAcrossDataSizesColumn : IColumn
{
    public string Id => nameof(AverageAcrossDataSizesColumn);
    public string ColumnName => "Avg Time";
    public string Legend => "Average execution time across all data sizes";
    public UnitType UnitType => UnitType.Time;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 1;
    public bool IsNumeric => true;
    public bool IsAvailable(Summary summary) => true;
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var storage = benchmarkCase.Parameters["Storage"];
        var method = benchmarkCase.Descriptor.WorkloadMethod.Name;

        var relatedReports = summary.Reports
            .Where(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == method &&
                       r.BenchmarkCase.Parameters["Storage"].ToString() == storage.ToString() &&
                       r.ResultStatistics != null)
            .ToList();

        if (relatedReports.Count == 0)
            return "N/A";

        var average = relatedReports.Average(r => r.ResultStatistics?.Mean ?? 0);
        return $"{average / 1_000_000:F2} ms";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        return GetValue(summary, benchmarkCase);
    }

    public override string ToString() => ColumnName;
}

/// <summary>
/// Custom column that computes a weighted score based on custom weights for different data sizes.
/// </summary>
public class WeightedScoreColumn : IColumn
{
    private readonly Dictionary<int, double> _weights;

    public WeightedScoreColumn(Dictionary<int, double> weights)
    {
        _weights = weights;
    }

    public string Id => nameof(WeightedScoreColumn);
    public string ColumnName => "Weighted Score";
    public string Legend => "Custom weighted score across data sizes";
    public UnitType UnitType => UnitType.Dimensionless;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 2;
    public bool IsNumeric => true;
    public bool IsAvailable(Summary summary) => true;
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var storage = benchmarkCase.Parameters["Storage"];
        var method = benchmarkCase.Descriptor.WorkloadMethod.Name;

        var relatedReports = summary.Reports
            .Where(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == method &&
                       r.BenchmarkCase.Parameters["Storage"].ToString() == storage.ToString() &&
                       r.ResultStatistics != null)
            .ToList();

        if (relatedReports.Count == 0)
            return "N/A";

        // Calculate weighted average
        double weightedSum = 0;
        double totalWeight = 0;

        foreach (var report in relatedReports)
        {
            var labels = (int)report.BenchmarkCase.Parameters["NumberOfLabels"];
            if (_weights.TryGetValue(labels, out var weight) && report.ResultStatistics?.Mean != null)
            {
                weightedSum += report.ResultStatistics.Mean * weight;
                totalWeight += weight;
            }
        }

        if (totalWeight == 0)
            return "N/A";

        var weightedAverage = weightedSum / totalWeight;
        return $"{weightedAverage / 1_000_000:F2} ms";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        return GetValue(summary, benchmarkCase);
    }

    public override string ToString() => ColumnName;
}

