using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReallyNotInstantService.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

var messages = new List<Message>();
var senderId = Guid.NewGuid();
var recipientId = Guid.NewGuid();

app.MapPut("/sendmessage", Results<Ok<Message>, BadRequest<string>>([FromBody] string messageContent) => {
    var message = new Message {
        Id = Guid.NewGuid(),
        SenderId = senderId,
        RecipientId = recipientId,
        MessageContent = messageContent,
        SentAtUtc = DateTime.UtcNow,
        Status = MessageStatusStrings.Sent
    };

    messages.Add(message);

    return TypedResults.Ok(message);
});

app.MapPost("/getmessagestatus", Results<Ok<string>, NotFound>([FromBody] string messageId) => {
    var message = messages.FirstOrDefault(m => m.Id.ToString() == messageId);

    if (message == null) {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(message.Status);
});

app.MapControllers();

app.Run();

