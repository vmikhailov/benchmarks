using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace TreeMap.Benchmarks;

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