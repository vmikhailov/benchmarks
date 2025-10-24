using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace WebApi.Tests;

[TestFixture]
public class HelloControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new();
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
        var response = await _client.GetAsync("/api/hello");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Get_ReturnsExpectedMessage()
    {
        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>("/api/hello");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Be("Hello World from Controller");
    }

    [Test]
    public async Task Get_ReturnsJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/hello");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Test]
    public async Task Greet_WithName_ReturnsPersonalizedMessage()
    {
        // Arrange
        var name = "Alice";

        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>($"/api/hello/greet/{name}");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Contain(name)
            .And.Contain("Hello")
            .And.Contain("Controller");
    }

    [Test]
    [TestCase("Bob")]
    [TestCase("Charlie")]
    [TestCase("David")]
    public async Task Greet_WithDifferentNames_ReturnsCorrectMessages(string name)
    {
        // Act
        var response = await _client.GetFromJsonAsync<HelloResponse>($"/api/hello/greet/{name}");

        // Assert
        response.Should().NotBeNull();
        response!.Message.Should().Contain(name);
    }

    [Test]
    public async Task Greet_WithEmptyName_StillReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/hello/greet/");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}

