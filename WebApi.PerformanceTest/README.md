# WebAPI Performance Testing Tool

A console application for measuring and comparing the performance of REST API endpoints across multiple environments.

## Features

- 📊 **Multi-URL Testing** - Test the same endpoints across different environments (localhost, dev, prod)
- 🎯 **Endpoint Agnostic** - Treats all endpoints equally (Controller-based, Minimal API, etc.)
- 🔄 **Concurrent Load Testing** - Configurable concurrent request handling
- 📈 **Comprehensive Metrics** - Average, Min, Max, Median, P95, P99 response times
- 🎨 **Rich Console Output** - Beautiful tables and charts using Spectre.Console
- 📉 **Performance Comparison** - Side-by-side comparison of endpoint performance

## Configuration

All test configuration is in `TestConfiguration.cs`:

```csharp
public static readonly List<string> BaseUrls = new()
{
    "http://localhost:5000",
    "https://localhost:5001",
    // Add more URLs for different environments
};

public static readonly List<EndpointInfo> Endpoints = new()
{
    new("Controller Hello", "/api/hello"),
    new("Minimal API Hello", "/api/hello-minimal"),
    new("Controller Greet", "/api/hello/greet/TestUser"),
    new("Minimal API Greet", "/api/hello-minimal/greet/TestUser"),
    // Add more endpoints here
};
```

## How to Use

### 1. Configure Endpoints
Edit `TestConfiguration.cs` to add your endpoints and base URLs.

### 2. Run the Tests
```bash
cd WebApi.PerformanceTest
dotnet run
```

### 3. Enter Test Parameters
The tool will prompt you for:
- **Number of requests per endpoint** (default: 1000)
- **Number of concurrent requests** (default: 10)

### 4. View Results
Results are displayed in multiple formats:
- Configuration summary table
- Detailed performance metrics per URL
- Comparison between first two endpoints
- Overall summary across all URLs

## Metrics Explained

| Metric | Description |
|--------|-------------|
| **Total Requests** | Total number of HTTP requests sent |
| **Success Rate** | Percentage of successful (2xx status) responses |
| **Avg Time (ms)** | Average response time in milliseconds |
| **Min (ms)** | Fastest response time |
| **Max (ms)** | Slowest response time |
| **Median (ms)** | 50th percentile response time |
| **P95 (ms)** | 95th percentile - 95% of requests were faster than this |
| **P99 (ms)** | 99th percentile - 99% of requests were faster than this |
| **RPS** | Requests per second (throughput) |

## Example Output

```
╔══════════════════════════════════════╗
║  Testing URL: http://localhost:5000  ║
╚══════════════════════════════════════╝

✓ Controller Hello completed
✓ Minimal API Hello completed
✓ Controller Greet completed
✓ Minimal API Greet completed

═══════════════════════════════════════════
  Results for http://localhost:5000
═══════════════════════════════════════════

┌─────────────────────────┬─────────┬──────────┬──────────┐
│ Endpoint                │ Avg (ms)│ P95 (ms) │ RPS      │
├─────────────────────────┼─────────┼──────────┼──────────┤
│ Controller Hello        │ 2.45    │ 5.23     │ 408.16   │
│ Minimal API Hello       │ 1.89    │ 4.12     │ 529.10   │
└─────────────────────────┴─────────┴──────────┴──────────┘

Performance Comparison
━━━━━━━━━━━━━━━━━━━━━━━━
Average Response Time: Minimal API is 22.86% faster
Requests Per Second: Minimal API handles 29.65% more requests/sec
```

## Adding New Endpoints

To add a new endpoint to test:

1. Open `TestConfiguration.cs`
2. Add a new entry to the `Endpoints` list:
   ```csharp
   new("Your Endpoint Name", "/your/endpoint/path")
   ```
3. Run the tool - it will automatically test the new endpoint

## Testing Multiple Environments

To test against multiple environments:

1. Open `TestConfiguration.cs`
2. Add URLs to the `BaseUrls` list:
   ```csharp
   public static readonly List<string> BaseUrls = new()
   {
       "http://localhost:5000",           // Local development
       "https://api-dev.example.com",     // Development server
       "https://api-staging.example.com", // Staging server
       "https://api.example.com"          // Production server
   };
   ```
3. Run the tool - it will test all endpoints against all URLs

## Performance Testing Tips

### For Accurate Results:
- Run the API in Release mode: `dotnet run --configuration Release`
- Close other applications to reduce system load
- Run multiple test iterations and compare results
- Test with different concurrent request levels (1, 10, 50, 100)
- Use realistic test data (actual user names, IDs, etc.)

### Interpreting Results:
- **Lower average time = faster endpoint**
- **Higher RPS = better throughput**
- **Lower P95/P99 = more consistent performance**
- **100% success rate = stable endpoint**

### Why Minimal API is Typically Faster:
- Less middleware overhead
- No controller instantiation
- Direct route mapping
- Fewer allocations
- Optimized lambda expressions

## Advanced Configuration

You can modify these constants in `TestConfiguration.cs`:

```csharp
public const int DefaultNumberOfRequests = 1000;
public const int DefaultConcurrentRequests = 10;
```

## Dependencies

- **Spectre.Console** - Rich console output and formatting
- **Microsoft.Extensions.Http** - HTTP client factory and extensions

## Troubleshooting

### Connection Refused Error
- Ensure the API is running before starting the tests
- Check that the URLs in `TestConfiguration.cs` are correct

### Low Performance
- Make sure you're testing in Release mode
- Increase concurrent requests to test throughput
- Check system resources (CPU, memory)

### High Failure Rate
- Verify endpoint paths are correct
- Check API logs for errors
- Reduce concurrent requests if overwhelming the server

