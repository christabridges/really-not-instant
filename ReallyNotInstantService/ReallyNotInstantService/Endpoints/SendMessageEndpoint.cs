using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReallyNotInstantService.Data;
using ReallyNotInstantService.Models;

namespace ReallyNotInstantService.Endpoints {

    public static class SendMessageEndpoint {

        public static void MapSendMessage(this IEndpointRouteBuilder app) {
            app.MapPut("/sendmessage", Results<Ok<Message>, BadRequest<string>> ([FromBody] string messageContent, IDeliveryTimeCalculator deliveryTimeCalculator, IPersistence<Message> messagesPersistence) => {
                var sender = new User("Rose", "Tyler", new Location(Latitude: 50.06239683723494, Longitude: -5.715337270597832));
                var recipient = new User("Donna", "Noble", new Location(Latitude: 58.638020261565, Longitude: -3.0689931908863137));

                var sentAtUtc = DateTime.UtcNow;
                var time = deliveryTimeCalculator.Calculate(sender.PostboxLocation, recipient.PostboxLocation, sentAtUtc).Result;
                var message = new Message {
                    Id = Guid.NewGuid(),
                    SenderId = sender.Id,
                    RecipientId = recipient.Id,
                    MessageContent = messageContent,
                    SentAtUtc = sentAtUtc,
                    Status = MessageStatusStrings.Sent,
                    PlannedDeliveryTimeUtc = time
                };

                messagesPersistence.Save(message);

                return TypedResults.Ok(message);
            });
        }
    }
}
