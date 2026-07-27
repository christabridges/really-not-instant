using FluentMigrator.Runner;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using ReallyNotInstantService.Data;
using ReallyNotInstantService.Data.Migrations;
using ReallyNotInstantService.Models;
using System.Net.Http.Headers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);



var rawConfig = new Configuration();
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
rawConfig.SetNamingStrategy(new PostgresNamingStrategy());

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

rawConfig.DataBaseIntegration(db => {
    db.ConnectionString = connectionString;
    db.Dialect<PostgreSQLDialect>();
    db.Driver<NpgsqlDriver>();
});

rawConfig.AddAssembly(Assembly.GetExecutingAssembly());
var sessionFactory = rawConfig.BuildSessionFactory();
builder.Services.AddSingleton(sessionFactory);
builder.Services.AddSingleton(_ => sessionFactory.OpenSession());
// Add services to the container.
builder.Services.AddScoped(sp => {
    var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openrouteservice.org") };
    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["OPEN_ROUTE_SERVICE_KEY"]);
    return httpClient;
});

builder.Services.AddScoped<IDistanceApiService>(sp => 
    new DistanceApiService(sp.GetRequiredService<HttpClient>())
);
builder.Services.AddScoped<IDeliveryTimeCalculator, DeliveryTimeCalculator>();
builder.Services.AddSingleton(typeof(IPersistence<>), typeof(Persistence<>));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints
var messages = new List<Message>();
var sender = new User("Rose", "Tyler", new Location(Latitude: 50.06239683723494, Longitude: -5.715337270597832));
var recipient = new User("Donna", "Noble", new Location(Latitude: 58.638020261565, Longitude: -3.0689931908863137));

app.MapPut("/sendmessage", Results<Ok<Message>, BadRequest<string>>([FromBody] string messageContent, IDeliveryTimeCalculator deliveryTimeCalculator, IPersistence<Message> messagesPersistence) => {
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

app.MapPost("/getmessagestatus", Results<Ok<string>, NotFound, BadRequest<string>>([FromBody] string messageId, IPersistence<Message> messagesPersistence) => {
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


app.UseAuthorization();

app.MapControllers();

app.Run();
