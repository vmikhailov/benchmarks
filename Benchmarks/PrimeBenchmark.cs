namespace Primes1;

public class PrimeBenchmark : IBenchmark
{
    public object Execute(int scale)
    {
        return PrimeCalculator.CalculatePrimes2(scale);
    }

    public int GetMetric(object result)
    {
        return ((List<int>)result).Count;
    }

    public string GetSample(object result)
    {
        var primes = (List<int>)result;
        var first20 = string.Join(", ", primes.Take(20));
        var last20 = string.Join(", ", primes.Skip(Math.Max(0, primes.Count - 20)));
        return $"First 20 primes: {first20}\nLast 20 primes: {last20}";
    }

    public string GetName() => "Prime Numbers";

    public string GetScaleUnit() => "limit";
}

