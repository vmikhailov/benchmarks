using Spectre.Console;

namespace WebApi.PerformanceTest;

public static class ResultsPresenter
{
    public static void DisplayResults(List<PerformanceResult> results)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Title("[bold yellow]API Performance Comparison Results[/]");

        // Add columns
        table.AddColumn("[bold]Endpoint[/]");
        table.AddColumn(new TableColumn("[bold]Total Requests[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Success Rate[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Avg Time (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Min (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Max (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Median (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]P95 (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]P99 (ms)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]RPS[/]").Centered());

        foreach (var result in results)
        {
            var successRate = (double)result.SuccessfulRequests / result.TotalRequests * 100;
            var successRateColor = successRate == 100 ? "green" : successRate > 95 ? "yellow" : "red";

            table.AddRow(
                $"[cyan]{result.EndpointName}[/]",
                result.TotalRequests.ToString(),
                $"[{successRateColor}]{successRate:F2}%[/]",
                $"{result.AverageTimeMs:F2}",
                $"{result.MinTimeMs:F2}",
                $"{result.MaxTimeMs:F2}",
                $"{result.MedianTimeMs:F2}",
                $"{result.P95TimeMs:F2}",
                $"{result.P99TimeMs:F2}",
                $"[bold]{result.RequestsPerSecond:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayComparison(PerformanceResult controller, PerformanceResult minimal)
    {
        AnsiConsole.WriteLine();
        var panel = new Panel(CreateComparisonText(controller, minimal))
        {
            Header = new("[bold green]Performance Comparison[/]"),
            Border = BoxBorder.Double,
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
    }

    private static string CreateComparisonText(PerformanceResult controller, PerformanceResult minimal)
    {
        var avgDiff = ((minimal.AverageTimeMs - controller.AverageTimeMs) / controller.AverageTimeMs) * 100;
        var rpsDiff = ((minimal.RequestsPerSecond - controller.RequestsPerSecond) / controller.RequestsPerSecond) * 100;

        var avgComparison = avgDiff > 0
            ? $"[red]Minimal API is {Math.Abs(avgDiff):F2}% slower[/]"
            : $"[green]Minimal API is {Math.Abs(avgDiff):F2}% faster[/]";

        var rpsComparison = rpsDiff > 0
            ? $"[green]Minimal API handles {Math.Abs(rpsDiff):F2}% more requests/sec[/]"
            : $"[red]Minimal API handles {Math.Abs(rpsDiff):F2}% fewer requests/sec[/]";

        return $"""
            [bold]Average Response Time:[/] {avgComparison}
            [bold]Requests Per Second:[/] {rpsComparison}
            
            [bold cyan]Controller Endpoint:[/]
              • Average: {controller.AverageTimeMs:F2} ms
              • Median: {controller.MedianTimeMs:F2} ms
              • P95: {controller.P95TimeMs:F2} ms
              • RPS: {controller.RequestsPerSecond:F2}
            
            [bold cyan]Minimal API Endpoint:[/]
              • Average: {minimal.AverageTimeMs:F2} ms
              • Median: {minimal.MedianTimeMs:F2} ms
              • P95: {minimal.P95TimeMs:F2} ms
              • RPS: {minimal.RequestsPerSecond:F2}
            """;
    }

    public static void DisplayHeader()
    {
        var rule = new Rule("[bold blue]API Performance Benchmarking Tool[/]")
        {
            Style = Style.Parse("blue")
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }
}

