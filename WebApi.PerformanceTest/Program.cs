using Spectre.Console;
using WebApi.PerformanceTest;

ResultsPresenter.DisplayHeader();

// Configuration from user input
var numberOfRequests = AnsiConsole.Prompt(
    new TextPrompt<int>("Enter number of requests per endpoint:")
        .DefaultValue(TestConfiguration.DefaultNumberOfRequests)
        .ValidationErrorMessage("[red]Please enter a valid positive number[/]")
        .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error()));

var concurrentRequests = AnsiConsole.Prompt(
    new TextPrompt<int>("Enter number of concurrent requests:")
        .DefaultValue(TestConfiguration.DefaultConcurrentRequests)
        .ValidationErrorMessage("[red]Please enter a valid positive number[/]")
        .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error()));

AnsiConsole.WriteLine();

// Display test configuration
var configTable = new Table()
    .Border(TableBorder.Rounded)
    .AddColumn("[bold]Configuration[/]")
    .AddColumn("[bold]Value[/]")
    .AddRow("Base URLs", TestConfiguration.BaseUrls.Count.ToString())
    .AddRow("Endpoint variations", TestConfiguration.Endpoints.Count.ToString())
    .AddRow("Total tests", (TestConfiguration.BaseUrls.Count * TestConfiguration.Endpoints.Count).ToString())
    .AddRow("Requests per test", numberOfRequests.ToString("N0"))
    .AddRow("Concurrent requests", concurrentRequests.ToString());

AnsiConsole.Write(configTable);
AnsiConsole.WriteLine();

var tester = new ApiPerformanceTester();
var allResults = new List<PerformanceResult>();

// Test each base URL with each endpoint variation
foreach (var baseUrl in TestConfiguration.BaseUrls)
{
    AnsiConsole.MarkupLine($"\n[bold cyan]╔══════════════════════════════════════════════════════╗[/]");
    AnsiConsole.MarkupLine($"[bold cyan]║  Testing: {baseUrl,-42} ║[/]");
    AnsiConsole.MarkupLine($"[bold cyan]╚══════════════════════════════════════════════════════╝[/]\n");
    
    foreach (var endpoint in TestConfiguration.Endpoints)
    {
        var fullUrl = baseUrl + endpoint.Path;
        var displayName = $"{baseUrl} - {endpoint.Name}";
        
        await AnsiConsole.Status()
            .StartAsync($"[yellow]Testing {endpoint.Name}...[/]", async _ =>
            {
                try
                {
                    var result = await tester.TestEndpointAsync(
                        displayName,
                        fullUrl,
                        numberOfRequests,
                        concurrentRequests);
                    allResults.Add(result);
                    AnsiConsole.MarkupLine($"[green]✓[/] {endpoint.Name} at {fullUrl} completed");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]✗[/] {endpoint.Name} failed: {ex.Message}");
                }
            });
    }
}

tester.Dispose();

AnsiConsole.WriteLine();

// Group results by base URL for display
var resultsByBaseUrl = allResults
    .GroupBy(r => r.EndpointName.Split(" - ")[0])
    .ToDictionary(g => g.Key, g => g.ToList());

// Display results for each base URL
foreach (var (url, results) in resultsByBaseUrl)
{
    if (results.Count == 0)
    {
        AnsiConsole.MarkupLine($"\n[bold red]No results for {url}[/]\n");
        continue;
    }

    AnsiConsole.MarkupLine($"\n[bold magenta]═══════════════════════════════════════════════════════[/]");
    AnsiConsole.MarkupLine($"[bold magenta]  Results for {url}[/]");
    AnsiConsole.MarkupLine($"[bold magenta]═══════════════════════════════════════════════════════[/]\n");
    
    ResultsPresenter.DisplayResults(results);
}

// Overall summary across all tests
AnsiConsole.WriteLine();
var summaryPanel = new Panel(CreateOverallSummary(allResults))
{
    Header = new("[bold magenta]📊 Overall Summary[/]"),
    Border = BoxBorder.Double,
    Padding = new Padding(2, 1)
};
AnsiConsole.Write(summaryPanel);

// Compare endpoints with same variation across different base URLs
if (TestConfiguration.BaseUrls.Count > 1 && TestConfiguration.Endpoints.Count > 0)
{
    AnsiConsole.WriteLine();
    DisplayCrossUrlComparison(allResults);
}

static string CreateOverallSummary(List<PerformanceResult> allResults)
{
    if (allResults.Count == 0)
    {
        return "[red]No test results available[/]";
    }
    
    var totalRequests = allResults.Sum(r => r.TotalRequests);
    var totalSuccessful = allResults.Sum(r => r.SuccessfulRequests);
    var totalFailed = allResults.Sum(r => r.FailedRequests);
    var overallSuccessRate = (double)totalSuccessful / totalRequests * 100;
    
    var fastestEndpoint = allResults.OrderBy(r => r.AverageTimeMs).First();
    var slowestEndpoint = allResults.OrderByDescending(r => r.AverageTimeMs).First();
    var highestThroughput = allResults.OrderByDescending(r => r.RequestsPerSecond).First();

    return $"""
        [bold]Total Tests Executed:[/] {allResults.Count}
        [bold]Total Requests:[/] {totalRequests:N0}
        [bold]Total Successful:[/] [green]{totalSuccessful:N0}[/]
        [bold]Total Failed:[/] {(totalFailed > 0 ? $"[red]{totalFailed:N0}[/]" : $"[green]{totalFailed}[/]")}
        [bold]Overall Success Rate:[/] {(overallSuccessRate >= 99 ? "[green]" : overallSuccessRate >= 95 ? "[yellow]" : "[red]")}{overallSuccessRate:F2}%[/]
        
        [bold green]⚡ Fastest:[/] {fastestEndpoint.EndpointName} ([green]{fastestEndpoint.AverageTimeMs:F2} ms[/])
        [bold red]🐌 Slowest:[/] {slowestEndpoint.EndpointName} ([red]{slowestEndpoint.AverageTimeMs:F2} ms[/])
        [bold green]🚀 Highest Throughput:[/] {highestThroughput.EndpointName} ([green]{highestThroughput.RequestsPerSecond:F2}[/] req/s)
        """;
}

static void DisplayCrossUrlComparison(List<PerformanceResult> allResults)
{
    var panel = new Panel(CreateCrossUrlComparisonText(allResults))
    {
        Header = new("[bold yellow]🔄 Cross-URL Performance Comparison[/]"),
        Border = BoxBorder.Rounded,
        Padding = new Padding(2, 1)
    };
    
    AnsiConsole.Write(panel);
}

static string CreateCrossUrlComparisonText(List<PerformanceResult> allResults)
{
    var comparisonLines = new List<string>();
    
    foreach (var endpoint in TestConfiguration.Endpoints)
    {
        var endpointResults = allResults
            .Where(r => r.EndpointName.EndsWith($" - {endpoint.Name}"))
            .OrderBy(r => r.AverageTimeMs)
            .ToList();
        
        if (endpointResults.Count > 1)
        {
            var fastest = endpointResults.First();
            var slowest = endpointResults.Last();
            var diff = ((slowest.AverageTimeMs - fastest.AverageTimeMs) / fastest.AverageTimeMs) * 100;
            
            comparisonLines.Add($"[bold cyan]{endpoint.Name}:[/]");
            comparisonLines.Add($"  Fastest: [green]{fastest.EndpointName.Split(" - ")[0]}[/] ({fastest.AverageTimeMs:F2} ms)");
            comparisonLines.Add($"  Slowest: [red]{slowest.EndpointName.Split(" - ")[0]}[/] ({slowest.AverageTimeMs:F2} ms, {diff:F1}% slower)");
            comparisonLines.Add("");
        }
    }
    
    return comparisonLines.Count > 0 
        ? string.Join("\n", comparisonLines.Take(comparisonLines.Count - 1))
        : "[yellow]Not enough data for comparison[/]";
}
