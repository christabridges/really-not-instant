using FluentMigrator.Runner;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using Npgsql;
using ReallyNotInstantService.Data;
using ReallyNotInstantService.Data.Migrations;
using ReallyNotInstantService.Endpoints;
using System.Net.Http.Headers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var rawConfig = new Configuration();
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
var convertedConnectionString = ConvertPostgresUrlToNpgsqlConnectionString(connectionString ?? "");
rawConfig.SetNamingStrategy(new PostgresNamingStrategy());

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(convertedConnectionString)
        .ScanIn(typeof(AddMessagesTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

rawConfig.DataBaseIntegration(db => {
    db.ConnectionString = convertedConnectionString;
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
builder.Services.AddScoped(typeof(IPersistence<>), typeof(Persistence<>));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var allowedOrigin = builder.Configuration["ALLOWED_ORIGIN"];

if (allowedOrigin == null) {
    throw new InvalidOriginException("Allowed origin needs to be provided in the environment variables");
}

var allowSpecificOriginsName = "allowSpecificOrigins";
builder.Services.AddCors(options => {
    options.AddPolicy(name: allowSpecificOriginsName,
        policy => {
            policy.WithOrigins(allowedOrigin);
            policy.WithMethods("PUT");
            policy.WithHeaders("content-type");
        });
});

var app = builder.Build();
var serviceScope = app.Services.CreateScope();
var services = serviceScope.ServiceProvider;
var runner = services.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseCors(allowSpecificOriginsName);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapMessageEndpoints();
app.MapControllers();


app.Run();

string ConvertPostgresUrlToNpgsqlConnectionString(string url) {
    if (url.StartsWith("Host=")) {
        return url;
    }

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    var builder = new NpgsqlConnectionStringBuilder { 
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };

    return builder.ToString();
}