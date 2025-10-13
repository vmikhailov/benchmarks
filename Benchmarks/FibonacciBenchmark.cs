namespace Primes1;

public class FibonacciBenchmark : IBenchmark
{
    public class FibonacciResult
    {
        public int TotalOperations { get; set; }
        public int RecursiveCalculations { get; set; }
        public int IterativeCalculations { get; set; }
        public int MemoizedCalculations { get; set; }
        public long SampleRecursiveResult { get; set; }
        public long SampleIterativeResult { get; set; }
        public long SampleMemoizedResult { get; set; }
    }

    // Classic recursive Fibonacci (inefficient but tests recursion overhead)
    private long FibonacciRecursive(int n)
    {
        if (n <= 1) return n;
        return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
    }

    // Iterative Fibonacci (efficient)
    private long FibonacciIterative(int n)
    {
        if (n <= 1) return n;
        
        long a = 0, b = 1;
        for (int i = 2; i <= n; i++)
        {
            long temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }

    // Memoized Fibonacci (dynamic programming)
    private long FibonacciMemoized(int n, Dictionary<int, long>? memo = null)
    {
        memo ??= new Dictionary<int, long>();
        
        if (n <= 1) return n;
        
        if (memo.ContainsKey(n))
            return memo[n];
        
        long result = FibonacciMemoized(n - 1, memo) + FibonacciMemoized(n - 2, memo);
        memo[n] = result;
        return result;
    }

    public object Execute(int scale)
    {
        var result = new FibonacciResult();

        // 1. Recursive Fibonacci (smaller values due to exponential complexity)
        // Calculate Fibonacci for values up to scale/10 (max ~35 for reasonable time)
        int maxRecursive = Math.Min(35, scale / 10);
        for (int i = 0; i <= maxRecursive; i++)
        {
            var fib = FibonacciRecursive(i);
            //var fib = 0;
            if (i == maxRecursive)
                result.SampleRecursiveResult = fib;
            result.RecursiveCalculations++;
        }

        // 2. Iterative Fibonacci (can handle much larger values)
        for (int i = 0; i < scale; i++)
        {
            var fib = FibonacciIterative(i % 90); // Keep under long overflow
            if (i == scale - 1)
                result.SampleIterativeResult = fib;
            result.IterativeCalculations++;
        }

        // 3. Memoized Fibonacci (efficient with caching)
        for (int i = 0; i < scale / 10; i++)
        {
            var memo = new Dictionary<int, long>();
            var fib = FibonacciMemoized(Math.Min(90, i + 20), memo);
            if (i == scale / 10 - 1)
                result.SampleMemoizedResult = fib;
            result.MemoizedCalculations++;
        }

        // 4. Multiple recursive calls to stress test
        for (int i = 20; i <= Math.Min(30, maxRecursive); i++)
        {
            var fib = FibonacciRecursive(i);
            result.RecursiveCalculations++;
        }

        // 5. Large iterative calculations
        for (int i = 0; i < scale / 100; i++)
        {
            var fib1 = FibonacciIterative(50);
            var fib2 = FibonacciIterative(75);
            var fib3 = FibonacciIterative(89);
            result.IterativeCalculations += 3;
        }

        // 6. Memoization with different cache sizes
        for (int i = 0; i < scale / 50; i++)
        {
            var memo = new Dictionary<int, long>();
            for (int j = 0; j < 50; j++)
            {
                var fib = FibonacciMemoized(j, memo);
                result.MemoizedCalculations++;
            }
        }

        result.TotalOperations = result.RecursiveCalculations + 
                                result.IterativeCalculations + 
                                result.MemoizedCalculations;

        return result;
    }

    public int GetMetric(object result)
    {
        return ((FibonacciResult)result).TotalOperations;
    }

    public string GetSample(object result)
    {
        var r = (FibonacciResult)result;
        return $"Total operations: {r.TotalOperations:N0}\n" +
               $"Recursive calculations: {r.RecursiveCalculations:N0}\n" +
               $"Iterative calculations: {r.IterativeCalculations:N0}\n" +
               $"Memoized calculations: {r.MemoizedCalculations:N0}\n" +
               $"Sample recursive result: {r.SampleRecursiveResult:N0}\n" +
               $"Sample iterative result: {r.SampleIterativeResult:N0}\n" +
               $"Sample memoized result: {r.SampleMemoizedResult:N0}";
    }

    public string GetName() => "Fibonacci Calculations";

    public string GetScaleUnit() => "calculations";
}

