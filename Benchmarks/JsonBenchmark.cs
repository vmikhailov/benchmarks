using System.Text.Json;

namespace Primes1;

public class JsonBenchmark : IBenchmark
{
    public class JsonResult
    {
        public List<ComplexData> Original { get; set; } = [];
        public string Json { get; set; } = "";
        public List<ComplexData>? Decoded { get; set; }
        public int Operations { get; set; }
    }

    public class ComplexData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Active { get; set; }
        public double Score { get; set; }
        public Metadata Metadata { get; set; } = new();
        public NestedData Nested { get; set; } = new();
    }

    public class Metadata
    {
        public string Created { get; set; } = "";
        public List<string> Tags { get; set; } = [];
        public Preferences Preferences { get; set; } = new();
    }

    public class Preferences
    {
        public string Theme { get; set; } = "";
        public string Language { get; set; } = "";
        public bool Notifications { get; set; }
    }

    public class NestedData
    {
        public Level1 Level1 { get; set; } = new();
    }

    public class Level1
    {
        public Level2 Level2 { get; set; } = new();
    }

    public class Level2
    {
        public Level3 Level3 { get; set; } = new();
    }

    public class Level3
    {
        public double Value { get; set; }
        public string Description { get; set; } = "";
    }

    public object Execute(int scale)
    {
        // Generate complex nested data structure
        var data = GenerateComplexData(scale);

        // Encode to JSON
        var jsonString = JsonSerializer.Serialize(data);

        // Decode back to verify round-trip
        var decoded = JsonSerializer.Deserialize<List<ComplexData>>(jsonString);

        return new JsonResult
        {
            Original = data,
            Json = jsonString,
            Decoded = decoded,
            Operations = scale * 2 // encode + decode per item
        };
    }

    private List<ComplexData> GenerateComplexData(int count)
    {
        var result = new List<ComplexData>(count);
        var random = new Random(42); // Fixed seed for consistency
        var languages = new[] { "en", "es", "fr", "de", "ja" };

        for (var i = 0; i < count; i++)
        {
            result.Add(new()
            {
                Id = i,
                Name = $"User_{i}",
                Email = $"user{i}@example.com",
                Active = i % 2 == 0,
                Score = Math.Round(random.Next(0, 10000) / 100.0, 2),
                Metadata = new()
                {
                    Created = DateTime.Now.AddSeconds(-random.Next(0, 31536000)).ToString("yyyy-MM-dd HH:mm:ss"),
                    Tags = [$"tag_{i % 10}", $"tag_{i % 5}", $"tag_{i % 3}"],
                    Preferences = new()
                    {
                        Theme = i % 2 == 0 ? "dark" : "light",
                        Language = languages[i % 5],
                        Notifications = i % 3 == 0
                    }
                },
                Nested = new()
                {
                    Level1 = new()
                    {
                        Level2 = new()
                        {
                            Level3 = new()
                            {
                                Value = i * 1.5,
                                Description = "Nested value at level 3"
                            }
                        }
                    }
                }
            });
        }

        return result;
    }

    public int GetMetric(object result)
    {
        return ((JsonResult)result).Operations;
    }

    public string GetSample(object result)
    {
        var r = (JsonResult)result;
        var jsonLength = r.Json.Length;
        var itemCount = r.Decoded?.Count ?? 0;
        var firstItem = itemCount > 0 ? JsonSerializer.Serialize(r.Decoded![0], new JsonSerializerOptions { WriteIndented = true }) : "";
        var lastItem = itemCount > 0 ? JsonSerializer.Serialize(r.Decoded![itemCount - 1], new JsonSerializerOptions { WriteIndented = true }) : "";

        return $"JSON string length: {jsonLength:N0} bytes\n" +
               $"Items processed: {itemCount:N0}\n" +
               $"First item:\n{(firstItem.Length > 200 ? firstItem.Substring(0, 200) + "..." : firstItem)}\n" +
               $"Last item:\n{(lastItem.Length > 200 ? lastItem.Substring(0, 200) + "..." : lastItem)}";
    }

    public string GetName() => "JSON Encoding/Decoding";

    public string GetScaleUnit() => "items";
}

