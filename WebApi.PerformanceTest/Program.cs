using Spectre.Console;
using WebApi.PerformanceTest;

ResultsPresenter.DisplayHeader();

// Configuration
var baseUrl = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter the API base URL:")
        .DefaultValue("http://localhost:5000")
        .PromptStyle("green"));

var numberOfRequests = AnsiConsole.Prompt(
    new TextPrompt<int>("Enter number of requests per endpoint:")
        .DefaultValue(1000)
        .ValidationErrorMessage("[red]Please enter a valid positive number[/]")
        .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error()));

var concurrentRequests = AnsiConsole.Prompt(
    new TextPrompt<int>("Enter number of concurrent requests:")
        .DefaultValue(10)
        .ValidationErrorMessage("[red]Please enter a valid positive number[/]")
        .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error()));

AnsiConsole.WriteLine();

var tester = new ApiPerformanceTester(baseUrl);
var results = new List<PerformanceResult>();

// Test Controller endpoint
await AnsiConsole.Status()
    .StartAsync("[yellow]Testing Controller endpoint...[/]", async ctx =>
    {
        var result = await tester.TestEndpointAsync(
            "Controller (/api/hello)",
            "/api/hello",
            numberOfRequests,
            concurrentRequests);
        results.Add(result);
        
        AnsiConsole.MarkupLine($"[green]✓[/] Controller endpoint test completed");
    });

// Test Minimal API endpoint
await AnsiConsole.Status()
    .StartAsync("[yellow]Testing Minimal API endpoint...[/]", async ctx =>
    {
        var result = await tester.TestEndpointAsync(
            "Minimal API (/api/hello-minimal)",
            "/api/hello-minimal",
            numberOfRequests,
            concurrentRequests);
        results.Add(result);
        
        AnsiConsole.MarkupLine($"[green]✓[/] Minimal API endpoint test completed");
    });

// Test Controller endpoint with parameter
await AnsiConsole.Status()
    .StartAsync("[yellow]Testing Controller endpoint with parameter...[/]", async ctx =>
    {
        var result = await tester.TestEndpointAsync(
            "Controller Greet (/api/hello/greet/TestUser)",
            "/api/hello/greet/TestUser",
            numberOfRequests,
            concurrentRequests);
        results.Add(result);
        
        AnsiConsole.MarkupLine($"[green]✓[/] Controller greet endpoint test completed");
    });

// Test Minimal API endpoint with parameter
await AnsiConsole.Status()
    .StartAsync("[yellow]Testing Minimal API endpoint with parameter...[/]", async ctx =>
    {
        var result = await tester.TestEndpointAsync(
            "Minimal API Greet (/api/hello-minimal/greet/TestUser)",
            "/api/hello-minimal/greet/TestUser",
            numberOfRequests,
            concurrentRequests);
        results.Add(result);
        
        AnsiConsole.MarkupLine($"[green]✓[/] Minimal API greet endpoint test completed");
    });

AnsiConsole.WriteLine();

// Display all results
ResultsPresenter.DisplayResults(results);

// Display comparison between Controller and Minimal API (basic endpoints)
if (results.Count >= 2)
{
    ResultsPresenter.DisplayComparison(results[0], results[1]);
}

// Summary statistics
AnsiConsole.WriteLine();
var summaryPanel = new Panel(CreateSummary(results))
{
    Header = new PanelHeader("[bold magenta]Summary[/]"),
    Border = BoxBorder.Rounded
};
AnsiConsole.Write(summaryPanel);

tester.Dispose();

static string CreateSummary(List<PerformanceResult> results)
{
    var totalRequests = results.Sum(r => r.TotalRequests);
    var totalSuccessful = results.Sum(r => r.SuccessfulRequests);
    var totalFailed = results.Sum(r => r.FailedRequests);
    var overallSuccessRate = (double)totalSuccessful / totalRequests * 100;
    
    var fastestEndpoint = results.OrderBy(r => r.AverageTimeMs).First();
    var highestThroughput = results.OrderByDescending(r => r.RequestsPerSecond).First();

    return $"""
        [bold]Total Requests:[/] {totalRequests:N0}
        [bold]Total Successful:[/] [green]{totalSuccessful:N0}[/]
        [bold]Total Failed:[/] {(totalFailed > 0 ? $"[red]{totalFailed:N0}[/]" : $"[green]{totalFailed}[/]")}
        [bold]Overall Success Rate:[/] {overallSuccessRate:F2}%
        
        [bold green]Fastest Endpoint:[/] {fastestEndpoint.EndpointName} ({fastestEndpoint.AverageTimeMs:F2} ms avg)
        [bold green]Highest Throughput:[/] {highestThroughput.EndpointName} ({highestThroughput.RequestsPerSecond:F2} req/s)
        """;
}


