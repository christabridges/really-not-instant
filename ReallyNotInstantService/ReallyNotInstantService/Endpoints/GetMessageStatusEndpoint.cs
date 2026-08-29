using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReallyNotInstantService.Data;
using ReallyNotInstantService.Models;

namespace ReallyNotInstantService.Endpoints {
    public static class GetMessageStatusEndpoint {
        public static void MapGetMessageStatus(this IEndpointRouteBuilder app) {
            app.MapPost("/getmessagestatus", Results<Ok<string>, NotFound, BadRequest<string>> ([FromBody] string messageId, IPersistence<Message> messagesPersistence) => {
                if (string.IsNullOrWhiteSpace(messageId)) {
                    return TypedResults.BadRequest("Message ID is required.");
                }

                if (!Guid.TryParse(messageId, out Guid messageGuid)) {
                    return TypedResults.BadRequest("Invalid Message ID format.");
                }

                var message = messagesPersistence.Read(messageGuid);

                if (message == null) {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(message.Status);
            });
        }
    }
}
