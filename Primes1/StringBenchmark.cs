using System.Text;

namespace Primes1;

public class StringBenchmark : IBenchmark
{
    public class StringResult
    {
        public int TotalOperations { get; set; }
        public int ConcatenationOps { get; set; }
        public int SplitOps { get; set; }
        public int JoinOps { get; set; }
        public int SearchOps { get; set; }
        public int FormatOps { get; set; }
        public int CaseConversionOps { get; set; }
        public int SubstringOps { get; set; }
        public int ReplaceOps { get; set; }
        public string SampleResult { get; set; } = "";
    }

    public object Execute(int scale)
    {
        var result = new StringResult();

        // 1. String concatenation operations
        var concat1 = "";

        for (int i = 0; i < scale / 100; i++)
        {
            concat1 += $"item_{i},";
            result.ConcatenationOps++;
        }

        // 2. StringBuilder operations (efficient concatenation)
        var sb = new StringBuilder(scale);

        for (int i = 0; i < scale / 10; i++)
        {
            sb.Append($"item_{i},");
            result.ConcatenationOps++;
        }

        var concat2 = sb.ToString();

        // 3. String splitting operations
        var testString = string.Join(",", Enumerable.Range(0, scale / 100).Select(i => $"word_{i}"));

        for (int i = 0; i < 100; i++)
        {
            var parts = testString.Split(',');
            result.SplitOps += parts.Length;
        }

        // 4. String joining operations
        var words = Enumerable.Range(0, scale / 100).Select(i => $"word_{i}").ToArray();

        for (int i = 0; i < 100; i++)
        {
            var joined = string.Join("|", words);
            result.JoinOps++;
        }

        //5. String searching operations
        var searchText = string.Join(" ",
            Enumerable.Range(0, 1000).Select(i => $"Lorem ipsum dolor sit amet word_{i} consectetur"));

        for (int i = 0; i < scale / 10; i++)
        {
            var index = searchText.IndexOf($"word_{i % 1000}", StringComparison.Ordinal);
            var contains = searchText.Contains($"word_{i % 1000}", StringComparison.Ordinal);
            result.SearchOps += 2;
        }

        // 6. String formatting operations
        for (int i = 0; i < scale; i++)
        {
            var formatted1 = string.Format("User {0}: {1} - Score: {2:F2}", i, $"Name_{i}", i * 1.5);
            var formatted2 = $"User {i}: Name_{i} - Score: {i * 1.5:F2}";
            result.FormatOps += 2;
        }

        //return result;

        // 7. Case conversion operations
        var mixedCaseStrings = Enumerable.Range(0, scale / 100)
            .Select(i => $"MixedCaseString_{i}_WithNumbers_{i * 2}")
            .ToArray();

        foreach (var str in mixedCaseStrings)
        {
            var upper = str.ToUpper();
            var lower = str.ToLower();
            result.CaseConversionOps += 2;
        }

        // 8. Substring operations
        var longString = string.Join("", Enumerable.Range(0, 10000).Select(i => $"segment_{i}_"));

        for (int i = 0; i < scale / 10; i++)
        {
            var start = (i * 13) % (longString.Length - 20);
            var sub = longString.Substring(start, 20);
            result.SubstringOps++;
        }

        // 9. String replacement operations
        var replaceText = string.Join(" ", Enumerable.Range(0, 1000).Select(i => $"test demo sample word_{i}"));

        for (int i = 0; i < scale / 10; i++)
        {
            var replaced = replaceText.Replace("test", "TEST", StringComparison.Ordinal)
                .Replace("demo", "DEMO", StringComparison.Ordinal)
                .Replace("sample", "SAMPLE", StringComparison.Ordinal);
            
            result.ReplaceOps += 3;
        }

        // 10. String trimming and padding
        var paddedStrings = Enumerable.Range(0, scale / 100)
            .Select(i => $"  padded_{i}  ")
            .ToArray();

        foreach (var str in paddedStrings)
        {
            var trimmed = str.Trim();
            var padded = trimmed.PadLeft(50, '*').PadRight(70, '*');
            result.TotalOperations += 3;
        }

        result.TotalOperations += result.ConcatenationOps + result.SplitOps +
                                  result.JoinOps + result.SearchOps +
                                  result.FormatOps + result.CaseConversionOps +
                                  result.SubstringOps + result.ReplaceOps;

        //result.SampleResult = concat2.Length > 100 ? concat2.Substring(0, 100) : concat2;

        return result;
    }

    public int GetMetric(object result)
    {
        return ((StringResult)result).TotalOperations;
    }

    public string GetSample(object result)
    {
        var r = (StringResult)result;

        return $"Total operations: {r.TotalOperations:N0}\n" +
               $"Concatenation ops: {r.ConcatenationOps:N0}\n" +
               $"Split ops: {r.SplitOps:N0}\n" +
               $"Join ops: {r.JoinOps:N0}\n" +
               $"Search ops: {r.SearchOps:N0}\n" +
               $"Format ops: {r.FormatOps:N0}\n" +
               $"Case conversion ops: {r.CaseConversionOps:N0}\n" +
               $"Substring ops: {r.SubstringOps:N0}\n" +
               $"Replace ops: {r.ReplaceOps:N0}\n" +
               $"Sample result: {r.SampleResult}";
    }

    public string GetName() => "String Operations";

    public string GetScaleUnit() => "operations";
}