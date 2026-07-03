using Microsoft.AspNetCore.Mvc;

namespace ReallyNotInstantService.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase {
        private static readonly List<Message> Messages = [];
        private List<Message> nonStaticMessages = [];
        private static Guid senderId = Guid.NewGuid();
        private static Guid recipientId = Guid.NewGuid();

        [HttpPut(Name = "SendMessage")]
        public IActionResult SendMessage([FromBody] string messageContent) {
            var message = new Message {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                MessageContent = messageContent,
                SentAtUtc = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };

            Messages.Add(message);
            this.nonStaticMessages.Add(message);
            return Ok();
        }

        public record Message {
            public required Guid Id;
            public required Guid SenderId;
            public required Guid RecipientId;
            public required DateTime SentAtUtc;
            public required string MessageContent;
            public required MessageStatus Status;
        }

        public enum MessageStatus { 
            Sent = 1,
            InTransit = 2,
            WaitingForDelivery = 3,
            Delivered = 4,
            Read = 5,
            Failed = 6
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
