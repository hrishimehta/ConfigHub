using ConfigHub.Client;
using ConfigHub.Shared.Options;
using MongoDB.Bson.Serialization.Conventions;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var certificatePath = Path.Combine(AppContext.BaseDirectory, "ConfigHubClientDll/SelfSignLocal.pfx");
var certificate = new X509Certificate2(certificatePath, "certificate_password");

builder.Services.AddConfigHubClient(new ConfigHubOptions
{
    UseConfigHubService = true,
    BaseAddress = "https://localhost:44315/", // Replace with actual ConfigHub API URL
    ApplicationName = "WeatherForecast", // Replace with your application ID
    ClientCertificate = certificate // Set your client certificate if required
});


//builder.Services.AddConfigHubClient(new ConfigHubOptions
//{
//    UseConfigHubService = false,
//    ConnectionString = "mongodb://localhost:27017/",
//    DatabaseName = "ConfigHub",
//    ConfigCollectionName = "Config",
//    ApplicationName = "WeatherForecast",
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
