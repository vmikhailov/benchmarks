using System;
using System.Collections;
using System.Diagnostics;

const int limit = 1_000_000_000;
const int testRuns = 20;

// Warmup run
Console.WriteLine("=== WARMUP RUN ===");
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

var primeFunc = CalculatePrimes;

var warmupWatch = Stopwatch.StartNew();
var warmupPrimes = primeFunc(limit/10);
warmupWatch.Stop();

Console.WriteLine(
    $"Warmup: Found {warmupPrimes.Count:N0} primes in {warmupWatch.ElapsedMilliseconds} ms ({warmupWatch.Elapsed.TotalSeconds:F3} seconds)");
Console.WriteLine();

// Prime Number Theorem comparison
Console.WriteLine("=== PRIME NUMBER THEOREM ===");
var actualCount = warmupPrimes.Count;
var pnt1 = limit / Math.Log(limit);
var pnt2 = limit / (Math.Log(limit) - 1);
var li = LogarithmicIntegral(limit);

Console.WriteLine($"Actual count:              {actualCount:N0}");

Console.WriteLine(
    $"π(n) ≈ n/ln(n):            {pnt1:N0} (error: {(actualCount - pnt1):N0}, {(actualCount - pnt1) / actualCount * 100:F3}%)");

Console.WriteLine(
    $"π(n) ≈ n/(ln(n)-1):        {pnt2:N0} (error: {(actualCount - pnt2):N0}, {(actualCount - pnt2) / actualCount * 100:F3}%)");

Console.WriteLine(
    $"Li(n) (Logarithmic Int.):  {li:N0} (error: {(actualCount - li):N0}, {(actualCount - li) / actualCount * 100:F3}%)");
Console.WriteLine();

// Test runs
Console.WriteLine($"=== RUNNING {testRuns} TEST ITERATIONS ===");
var executionTimes = new List<long>();
var memoryUsages = new List<long>();

for (int run = 1; run <= testRuns; run++)
{
    // Force garbage collection before each run
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var memoryBefore = GC.GetTotalMemory(false);
    var stopwatch = Stopwatch.StartNew();

    var primes = primeFunc(limit);

    stopwatch.Stop();
    var memoryAfter = GC.GetTotalMemory(false);
    var memoryUsed = memoryAfter - memoryBefore;

    executionTimes.Add(stopwatch.ElapsedMilliseconds);
    memoryUsages.Add(memoryUsed);

    Console.WriteLine($"\rRun {run,2}: {stopwatch.ElapsedMilliseconds,5} ms ({stopwatch.Elapsed.TotalSeconds:F3} sec) | Memory: {memoryUsed / 1024.0 / 1024.0,6:F2} MB");
}

Console.WriteLine();
Console.WriteLine("=== STATISTICS ===");
Console.WriteLine($"Execution Time:");
Console.WriteLine($"  Average:  {executionTimes.Average(),7:F2} ms");
Console.WriteLine($"  Min:      {executionTimes.Min(),7} ms");
Console.WriteLine($"  Max:      {executionTimes.Max(),7} ms");
Console.WriteLine($"  Median:   {GetMedian(executionTimes),7:F2} ms");
Console.WriteLine($"  StdDev:   {GetStandardDeviation(executionTimes),7:F2} ms");
Console.WriteLine();
Console.WriteLine($"Memory Usage:");
Console.WriteLine($"  Average:  {memoryUsages.Average() / 1024.0 / 1024.0,7:F2} MB");
Console.WriteLine($"  Min:      {memoryUsages.Min() / 1024.0 / 1024.0,7:F2} MB");
Console.WriteLine($"  Max:      {memoryUsages.Max() / 1024.0 / 1024.0,7:F2} MB");
Console.WriteLine($"  Median:   {GetMedian(memoryUsages) / 1024.0 / 1024.0,7:F2} MB");
Console.WriteLine();

// Display first and last 20 primes from the last run
// Console.WriteLine("First 20 primes:");
// for (int i = 0; i < Math.Min(20, warmupPrimes.Count); i++)
// {
//     Console.Write($"{warmupPrimes[i]} ");
// }
// Console.WriteLine();
// Console.WriteLine();
//
// Console.WriteLine("Last 20 primes:");
// for (int i = Math.Max(0, warmupPrimes.Count - 20); i < warmupPrimes.Count; i++)
// {
//     Console.Write($"{warmupPrimes[i]} ");
// }
// Console.WriteLine();

static List<int> CalculatePrimes(int max)
{
    if (max < 2) return [];

    // Sieve of Eratosthenes
    var isPrime = new bool[max + 1];
    //var isPrime = new BitArray(max + 1);

    for (var i = 2; i <= max; i++)
    {
        isPrime[i] = true;
    }

    for (var i = 2; i * i <= max; i++)
    {
        if (!isPrime[i])
        {
            continue;
        }

        for (var j = i * i; j <= max; j += i)
        {
            isPrime[j] = false;
        }
    }

    //var c = LogarithmicIntegral(max);
    var c = 0;
    var primes = new List<int>((int)c);

    for (var i = 2; i <= max; i++)
    {
        if (isPrime[i])
        {
            primes.Add(i);
        }
    }

    return primes;
}

static List<int> CalculatePrimes2(int n)
{
    if (n <= 2) return [];

    var n2 = n / 2;

    var marks = new BitArray(n2);
    //var marks = new bool[n2];

    var c = 1;

    for (var k = 1; k < n2; k++)
    {
        if (marks[k]) continue;

        for (var v = 3 * k + 1; v < n2; v += 2 * k + 1)
        {
            marks[v] = true;
        }

        c++;
    }

    var primes = new List<int>(c) { 2 };

    for (var i = 1; i < n2; i++)
    {
        if (!marks[i])
        {
            primes.Add(i * 2 + 1);
        }
    }

    return primes;
}

static double LogarithmicIntegral(int n)
{
    // Approximation of Li(n) using numerical integration (Simpson's rule)
    // Li(n) = integral from 2 to n of 1/ln(t) dt
    if (n < 2) return 0;

    const int steps = 10000;
    double h = (n - 2.0) / steps;
    double sum = 1.0 / Math.Log(2) + 1.0 / Math.Log(n);

    for (int i = 1; i < steps; i++)
    {
        double t = 2.0 + i * h;
        double weight = (i % 2 == 0) ? 2.0 : 4.0;
        sum += weight / Math.Log(t);
    }

    return (h / 3.0) * sum;
}

static double GetMedian(List<long> values)
{
    var sorted = values.OrderBy(x => x).ToList();
    var count = sorted.Count;

    if (count % 2 == 0)
    {
        return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
    }
    else
    {
        return sorted[count / 2];
    }
}

static double GetStandardDeviation(List<long> values)
{
    double avg = values.Average();
    double sumOfSquares = values.Sum(val => (val - avg) * (val - avg));
    return Math.Sqrt(sumOfSquares / values.Count);
}