using Microsoft.AspNetCore.Mvc;

namespace ReallyNotInstantService.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase {
        private static readonly List<string> Messages = [];
        private List<string> NonStaticMessages = [];

        [HttpPut(Name = "SendMessage")]
        public IActionResult SendMessage([FromBody] string message) {
            Messages.Add(message);
            this.NonStaticMessages.Add(message);
            return Ok();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get() {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
