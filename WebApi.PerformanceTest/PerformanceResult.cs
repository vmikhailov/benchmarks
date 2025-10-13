namespace WebApi.PerformanceTest;

public class PerformanceResult
{
    public string EndpointName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double TotalTimeMs { get; set; }
    public double AverageTimeMs { get; set; }
    public double MinTimeMs { get; set; }
    public double MaxTimeMs { get; set; }
    public double RequestsPerSecond { get; set; }
    public double MedianTimeMs { get; set; }
    public double P95TimeMs { get; set; }
    public double P99TimeMs { get; set; }
}

