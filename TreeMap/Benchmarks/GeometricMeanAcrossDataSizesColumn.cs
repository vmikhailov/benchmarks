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
