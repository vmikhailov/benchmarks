namespace Primes1;

public class DictionaryBenchmark : IBenchmark
{
    public class DictionaryResult
    {
        public int TotalOperations { get; set; }
        public int InsertCount { get; set; }
        public int LookupCount { get; set; }
        public int UpdateCount { get; set; }
        public int DeleteCount { get; set; }
        public int IterationCount { get; set; }
        public int ContainsKeyCount { get; set; }
        public Dictionary<string, object> SampleData { get; set; } = new();
    }

    public object Execute(int scale)
    {
        var result = new DictionaryResult();
        var dict = new Dictionary<string, object>(scale);

        // 1. Insert operations
        for (int i = 0; i < scale; i++)
        {
            dict[$"key_{i}"] = new
            {
                Id = i,
                Name = $"Item_{i}",
                Value = i * 1.5,
                Active = i % 2 == 0,
                Tags = new[] { $"tag_{i % 10}", $"category_{i % 5}" },
                Metadata = new Dictionary<string, object>
                {
                    ["created"] = DateTime.Now.AddDays(-i%1000),
                    ["priority"] = i % 3,
                    ["score"] = (double)i / scale * 100
                }
            };
            result.InsertCount++;
        }

        // 2. Lookup operations
        for (int i = 0; i < scale; i += 2)
        {
            var key = $"key_{i}";
            if (dict.TryGetValue(key, out var value))
            {
                result.LookupCount++;
            }
        }

        // 3. ContainsKey operations
        for (int i = 0; i < scale; i += 5)
        {
            if (dict.ContainsKey($"key_{i}"))
            {
                result.ContainsKeyCount++;
            }
        }

        // 4. Update operations
        for (int i = 0; i < scale; i += 3)
        {
            var key = $"key_{i}";
            if (dict.ContainsKey(key))
            {
                dict[key] = new
                {
                    Id = i,
                    Name = $"Updated_{i}",
                    Value = i * 2.0,
                    Active = true,
                    Tags = new[] { $"updated_{i}" },
                    Metadata = new Dictionary<string, object>
                    {
                        ["modified"] = DateTime.Now,
                        ["version"] = 2
                    }
                };
                result.UpdateCount++;
            }
        }

        // 5. Iteration operations
        foreach (var kvp in dict)
        {
            // Simulate processing
            var key = kvp.Key;
            var value = kvp.Value;
            result.IterationCount++;
        }

        // 6. Delete operations
        for (int i = 0; i < scale; i += 10)
        {
            if (dict.Remove($"key_{i}"))
            {
                result.DeleteCount++;
            }
        }

        // 7. Complex operations - filtering and transforming
        var filteredKeys = dict.Keys
            .Where(k => int.Parse(k.Split('_')[1]) % 7 == 0)
            .Take(100)
            .ToList();

        // 8. Bulk operations
        var newDict = new Dictionary<string, object>(filteredKeys.Count);
        foreach (var key in filteredKeys)
        {
            if (dict.TryGetValue(key, out var value))
            {
                newDict[key] = value;
            } 
        }

        result.TotalOperations = result.InsertCount + result.LookupCount + 
                                result.UpdateCount + result.DeleteCount + 
                                result.IterationCount + result.ContainsKeyCount;

        // Store sample data for display
        result.SampleData = dict.Take(5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return result;
    }

    public int GetMetric(object result)
    {
        return ((DictionaryResult)result).TotalOperations;
    }

    public string GetSample(object result)
    {
        var r = (DictionaryResult)result;
        return $"Total operations: {r.TotalOperations:N0}\n" +
               $"Insert operations: {r.InsertCount:N0}\n" +
               $"Lookup operations: {r.LookupCount:N0}\n" +
               $"Update operations: {r.UpdateCount:N0}\n" +
               $"Delete operations: {r.DeleteCount:N0}\n" +
               $"Iteration count: {r.IterationCount:N0}\n" +
               $"ContainsKey operations: {r.ContainsKeyCount:N0}\n" +
               $"Sample keys: {string.Join(", ", r.SampleData.Keys.Take(3))}";
    }

    public string GetName() => "Dictionary Operations";

    public string GetScaleUnit() => "items";
}

