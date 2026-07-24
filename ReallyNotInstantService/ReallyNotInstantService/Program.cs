using FluentMigrator.Runner;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReallyNotInstantService.Data.Migrations;
using ReallyNotInstantService.Models;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddScoped(sp => {
    var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openrouteservice.org") };
    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["OPENROUTESERVICEKEY"]);
    return httpClient;
});

builder.Services.AddScoped<IDistanceApiService>(sp => 
    new DistanceApiService(sp.GetRequiredService<HttpClient>())
);
builder.Services.AddScoped<IDeliveryTimeCalculator, DeliveryTimeCalculator>();

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
var serviceProvider = builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(AddMessagesTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    .BuildServiceProvider();

using (var scope = serviceProvider.CreateScope()) {
    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

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



// Endpoints
var messages = new List<Message>();
var sender = new User("Rose", "Tyler", new Location(Latitude: 50.06239683723494, Longitude: -5.715337270597832));
var recipient = new User("Donna", "Noble", new Location(Latitude: 58.638020261565, Longitude: -3.0689931908863137));

app.MapPut("/sendmessage", Results<Ok<Message>, BadRequest<string>>([FromBody] string messageContent, IDeliveryTimeCalculator deliveryTimeCalculator) => {
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
