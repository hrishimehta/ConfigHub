using ConfigHub.Client;
using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfigHubClient configHubClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfigHubClient configHubClient)
    {
        _logger = logger;
        this.configHubClient = configHubClient;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var weatherProviderUrl = await configHubClient.GetConfigItemByKeyAndComponent("WeatherAPI", "WeatherProviderUrl");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
