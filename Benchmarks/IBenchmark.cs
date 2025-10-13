namespace Primes1;

public interface IBenchmark
{
    /// <summary>
    /// Execute the benchmark test
    /// </summary>
    /// <param name="scale">Scale/size parameter for the test</param>
    /// <returns>Result of the benchmark</returns>
    object Execute(int scale);

    /// <summary>
    /// Get a count or metric from the result
    /// </summary>
    int GetMetric(object result);

    /// <summary>
    /// Get a sample of the result for display
    /// </summary>
    string GetSample(object result);

    /// <summary>
    /// Get the name of this benchmark
    /// </summary>
    string GetName();

    /// <summary>
    /// Get the unit for the scale parameter
    /// </summary>
    string GetScaleUnit();
}

