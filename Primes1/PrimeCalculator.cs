using System.Collections;

namespace Primes1;

public class PrimeCalculator
{
    /// <summary>
    /// Calculate all prime numbers up to a given limit using the Sieve of Eratosthenes algorithm
    /// </summary>
    public static List<int> CalculatePrimes(int max)
    {
        if (max < 2) return [];

        // Sieve of Eratosthenes
        var isPrime = new bool[max + 1];

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

        var primes = new List<int>();

        for (var i = 2; i <= max; i++)
        {
            if (isPrime[i])
            {
                primes.Add(i);
            }
        }

        return primes;
    }

    /// <summary>
    /// Calculate primes using a more memory-efficient approach (odd numbers only)
    /// </summary>
    public static List<int> CalculatePrimes2(int n)
    {
        if (n <= 2) return n == 2 ? [2] : [];

        var n2 = n / 2;
        var marks = new BitArray(n2);

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
}

