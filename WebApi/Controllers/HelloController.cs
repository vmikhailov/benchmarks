using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        //await Task.Delay(100);
        return Ok(new { message = "Hello World from Controller" });
    }
    
    [HttpGet("greet/{name}")]
    public IActionResult Greet(string name)
    {
        return Ok(new { message = $"Hello, {name}! (from Controller)" });
    }
}
