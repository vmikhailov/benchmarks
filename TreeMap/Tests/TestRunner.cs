namespace TreeMap.Tests;

/// <summary>
/// Central test runner for all MapStorage implementations.
/// </summary>
public static class TestRunner
{
    public static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        var implementation = args[1].ToLower();

        switch (implementation)
        {
            case "dictionary":
            case "dict":
                DictionaryTest.RunTests();
                break;

            case "bst":
                BstTest.RunTests();
                break;

            case "sortedarray":
            case "array":
                SortedArrayTest.RunTests();
                break;

            case "sorteddict":
            case "sorteddictionary":
                SortedDictionaryTest.RunTests();
                break;

            case "tiled":
                RunTiledTest(args);
                break;

            case "dynamictiled":
            case "dynamic":
                RunDynamicTiledTest(args);
                break;

            case "all":
                RunAllTests();
                break;

            default:
                Console.WriteLine($"Unknown implementation: {implementation}");
                PrintUsage();
                break;
        }
    }

    private static void RunTiledTest(string[] args)
    {
        // Parse optional tile size parameter: "test tiled 16" or "test tiled 16 perf"
        int tileSize = 16; // default
        bool runPerformance = false;

        if (args.Length > 2)
        {
            if (int.TryParse(args[2], out var parsedSize))
            {
                tileSize = parsedSize;
            }
            else if (args[2].Equals("perf", StringComparison.OrdinalIgnoreCase) ||
                     args[2].Equals("performance", StringComparison.OrdinalIgnoreCase))
            {
                runPerformance = true;
            }
        }

        if (args.Length > 3)
        {
            if (args[3].Equals("perf", StringComparison.OrdinalIgnoreCase) ||
                args[3].Equals("performance", StringComparison.OrdinalIgnoreCase))
            {
                runPerformance = true;
            }
        }

        TiledTest.RunTests(tileSize);

        if (runPerformance)
        {
            Console.WriteLine();
            TiledTest.RunPerformanceTests(tileSize);
        }
    }

    private static void RunDynamicTiledTest(string[] args)
    {
        // Parse optional capacity parameter: "test dynamic 64" or "test dynamic 64 perf"
        int capacity = 64; // default
        bool runPerformance = false;

        if (args.Length > 2)
        {
            if (int.TryParse(args[2], out var parsedCapacity))
            {
                capacity = parsedCapacity;
            }
            else if (args[2].Equals("perf", StringComparison.OrdinalIgnoreCase) ||
                     args[2].Equals("performance", StringComparison.OrdinalIgnoreCase))
            {
                runPerformance = true;
            }
        }

        if (args.Length > 3)
        {
            if (args[3].Equals("perf", StringComparison.OrdinalIgnoreCase) ||
                args[3].Equals("performance", StringComparison.OrdinalIgnoreCase))
            {
                runPerformance = true;
            }
        }

        DynamicTiledTest.RunTests(capacity);

        if (runPerformance)
        {
            Console.WriteLine();
            DynamicTiledTest.RunPerformanceTests(capacity);
        }
    }

    private static void RunAllTests()
    {
        Console.WriteLine("=== Running All Implementation Tests ===\n");

        DictionaryTest.RunTests();
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        BstTest.RunTests();
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        SortedArrayTest.RunTests();
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        SortedDictionaryTest.RunTests();
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        TiledTest.RunTests(16);
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        DynamicTiledTest.RunTests(64);
        Console.WriteLine("\n" + new string('=', 60) + "\n");

        Console.WriteLine("✓ All tests completed successfully!");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run -- test <implementation> [options]");
        Console.WriteLine();
        Console.WriteLine("Implementations:");
        Console.WriteLine("  dictionary, dict         - Test Dictionary implementation");
        Console.WriteLine("  bst                      - Test Binary Search Tree implementation");
        Console.WriteLine("  sortedarray, array       - Test Sorted Array implementation");
        Console.WriteLine("  sorteddict               - Test Sorted Dictionary implementation");
        Console.WriteLine("  tiled [size] [perf]      - Test Tiled implementation");
        Console.WriteLine("                             Optional: tile size (default: 16)");
        Console.WriteLine("                             Optional: 'perf' for performance tests");
        Console.WriteLine("  dynamictiled, dynamic    - Test Dynamic Tiled implementation");
        Console.WriteLine("    [capacity] [perf]        Optional: tile capacity (default: 64)");
        Console.WriteLine("                             Optional: 'perf' for performance tests");
        Console.WriteLine("  all                      - Run all tests");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- test dictionary");
        Console.WriteLine("  dotnet run -- test tiled 16");
        Console.WriteLine("  dotnet run -- test tiled 16 perf");
        Console.WriteLine("  dotnet run -- test dynamic 64");
        Console.WriteLine("  dotnet run -- test dynamic 64 perf");
        Console.WriteLine("  dotnet run -- test all");
    }
}

