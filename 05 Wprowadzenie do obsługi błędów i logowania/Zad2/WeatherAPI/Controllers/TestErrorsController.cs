using Microsoft.AspNetCore.Mvc;

namespace WeatherAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class TestErrorsController : ControllerBase
{
    private readonly ILogger<TestErrorsController> _logger;

    public TestErrorsController(ILogger<TestErrorsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Wywolano endpoint testujacy logowanie i obsluge bledow.");

        throw new Exception("Testowy wyjątek");
    }
}
