namespace Primes1;

public class BenchmarkResult
{
    public string BenchmarkName { get; set; } = "";
    public double AvgTime { get; set; }
    public double MinTime { get; set; }
    public double MaxTime { get; set; }
    public double MedianTime { get; set; }
    public double StdDev { get; set; }
    public double AvgMemory { get; set; }
    public double MinMemory { get; set; }
    public double MaxMemory { get; set; }
    public int Metric { get; set; }
}