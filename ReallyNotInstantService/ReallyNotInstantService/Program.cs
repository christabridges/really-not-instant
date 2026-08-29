using FluentMigrator.Runner;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using ReallyNotInstantService.Data;
using ReallyNotInstantService.Data.Migrations;
using ReallyNotInstantService.Endpoints;
using System.Net.Http.Headers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var rawConfig = new Configuration();
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
rawConfig.SetNamingStrategy(new PostgresNamingStrategy());

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(AddMessagesTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

rawConfig.DataBaseIntegration(db => {
    db.ConnectionString = connectionString;
    db.Dialect<PostgreSQLDialect>();
    db.Driver<NpgsqlDriver>();
});

rawConfig.AddAssembly(Assembly.GetExecutingAssembly());
var sessionFactory = rawConfig.BuildSessionFactory();
builder.Services.AddSingleton(sessionFactory);
builder.Services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());

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
var migrator = app.Services.GetRequiredService<IMigrationRunner>();
migrator.MigrateUp();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapMessageEndpoints();
app.MapControllers();

app.Run();
