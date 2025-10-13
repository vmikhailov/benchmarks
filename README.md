# WebAPI Test Project

This project contains comprehensive integration tests for a REST API with both Controller-based and Minimal API endpoints.

## Technologies Used

- **ASP.NET Core Web API** - The web framework
- **NUnit** - Testing framework
- **FluentAssertions** - Assertion library for more readable tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support

## Project Structure

```
WebApi/
  ├── Controllers/
  │   └── HelloController.cs      # Controller-based endpoint
  └── Program.cs                   # Minimal API endpoints

WebApi.Tests/
  ├── HelloControllerTests.cs      # Tests for controller endpoint
  ├── HelloMinimalApiTests.cs      # Tests for minimal API endpoint
  └── Models/
      └── HelloResponse.cs         # Shared response model
```

## API Endpoints

### Controller-Based Endpoints
- `GET /api/hello` - Returns a hello world message from the controller
- `GET /api/hello/greet/{name}` - Returns a personalized greeting from the controller

### Minimal API Endpoints
- `GET /api/hello-minimal` - Returns a hello world message from minimal API
- `GET /api/hello-minimal/greet/{name}` - Returns a personalized greeting from minimal API

## Running the Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run only controller tests
dotnet test --filter FullyQualifiedName~HelloControllerTests

# Run only minimal API tests
dotnet test --filter FullyQualifiedName~HelloMinimalApiTests
```

## Test Features

The test suite demonstrates:

1. **HTTP Status Code Validation** - Ensuring endpoints return correct status codes
2. **Response Content Validation** - Verifying JSON responses contain expected data
3. **Content-Type Validation** - Checking response headers
4. **Parameterized Tests** - Using TestCase attributes to test multiple inputs
5. **Concurrent Request Testing** - Testing API behavior under concurrent load
6. **FluentAssertions** - Using expressive assertion syntax for better readability

## Example Test with FluentAssertions

```csharp
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
```

## Building and Running the API

```bash
# Build the API
cd WebApi
dotnet build

# Run the API
dotnet run

# The API will be available at https://localhost:5001 (or http://localhost:5000)
```

## Adding New Tests

When adding new tests:

1. Follow the AAA pattern (Arrange, Act, Assert)
2. Use descriptive test names that explain what is being tested
3. Use FluentAssertions for readable assertions
4. Leverage `[TestCase]` for parameterized tests
5. Use `SetUp` and `TearDown` for proper resource management

