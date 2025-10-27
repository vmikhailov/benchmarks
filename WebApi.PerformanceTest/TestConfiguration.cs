namespace WebApi.PerformanceTest;

public static class TestConfiguration
{
    public static readonly List<string> BaseUrls = new()
    {
        "http://localhost:5000/api/hello",
       // "http://localhost:5000/api/hello-minimal",
        "http://localhost:5001/api/hello",
       // "http://localhost:5001/api/hello-minimal",
    };

    public static readonly List<EndpointInfo> Endpoints = new()
    {
        new("Hello", ""),
        //new("Greet", "/greet/TestUser"),
    };

    public const int DefaultNumberOfRequests = 1000;
    public const int DefaultConcurrentRequests = 10;
}