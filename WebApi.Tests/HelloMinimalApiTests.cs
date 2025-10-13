using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace WebApi.Tests;

[TestFixture]
public class HelloMinimalApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Get_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/hello-minimal");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Get_ReturnsExpectedMessage()
    {
        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>("/api/hello-minimal");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Be("Hello World from Minimal API");
    }

    [Test]
    public async Task Get_ReturnsJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/hello-minimal");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Test]
    public async Task Get_ResponseStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/hello-minimal");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().ContainAll("message", "Hello World");
    }

    [Test]
    public async Task Greet_WithName_ReturnsPersonalizedMessage()
    {
        // Arrange
        var name = "Alice";

        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>($"/api/hello-minimal/greet/{name}");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should()
            .Contain(name)
            .And.Contain("Hello")
            .And.Contain("Minimal API");
    }

    [Test]
    [TestCase("Bob")]
    [TestCase("Charlie")]
    [TestCase("David")]
    [TestCase("Mary-Jane")]
    public async Task Greet_WithDifferentNames_ReturnsCorrectMessages(string name)
    {
        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>($"/api/hello-minimal/greet/{name}");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Contain(name);
    }

    [Test]
    public async Task Greet_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var name = "Mary-Jane";

        // Act
        var response = await _client.GetAsync($"/api/hello-minimal/greet/{name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HelloResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("Mary-Jane");
    }

    [Test]
    public async Task MultipleRequests_ShouldAllSucceed()
    {
        // Arrange
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _client.GetAsync("/api/hello-minimal"))
            .ToArray();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }
}

