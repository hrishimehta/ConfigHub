using ConfigHub.API.Middleware;
using ConfigHub.Infrastructure.Interface;
using ConfigHub.Infrastructure.Services;
using ConfigHub.Mongo.Interface;
using ConfigHub.Mongo.Services;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });


});

// Register the custom convention pack
var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true), // Ignore extra elements during deserialization
            new IgnoreIfNullConvention(true) // Ignore null properties during serialization
        };

ConventionRegistry.Register("MyConventionPack", conventionPack, t => true);


builder.Services.AddSingleton<IMongoRepositoryFactory, MongoRepositoryFactory>(provider => new MongoRepositoryFactory(GetMongoConnectionString(provider.GetService<IConfiguration>())));
builder.Services.AddTransient(typeof(IMongoRepository<>), typeof(GenericMongoRepository<>));
builder.Services.AddTransient<IConfigService, ConfigService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CertificateAuthenticationMiddleware>();



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



string GetMongoConnectionString(IConfiguration? configuration)
{
    return configuration.GetSection("MongoDBSettings:ConnectionString").Value.ToString();
}
