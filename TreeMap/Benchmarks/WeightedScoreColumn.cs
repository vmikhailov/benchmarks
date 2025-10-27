using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace TreeMap.Benchmarks;

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