using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Services;
using ConfigHub.Mongo.Interface;
using ConfigHub.Mongo.Services;
using ConfigHub.Shared.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using MongoDB.Bson.Serialization.Conventions;
using System;

namespace ConfigHub.Client
{
    public static class ConfigHubClientExtensions
    {

        public static void AddConfigHubClient(this IServiceCollection services, ConfigHubOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Register the custom convention pack
            var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true), // Ignore extra elements during deserialization
            new IgnoreIfNullConvention(true) // Ignore null properties during serialization
        };

            ConventionRegistry.Register("MyConventionPack", conventionPack, t => true);

            if (options.UseConfigHubService)
            {

                if (string.IsNullOrEmpty(options.BaseAddress) || options.ClientCertificate == null || string.IsNullOrEmpty(options.ApplicationName))
                {
                    throw new InvalidOperationException("ConfigHub base address, client certificate and ApplicaitonId must be provided when using ConfigHubServiceClient.");
                }

                services.AddSingleton(options);
                services.AddScoped<IConfigHubClient, ConfigHubServiceClient>();
            }
            else
            {
                if (string.IsNullOrEmpty(options.ConnectionString) || string.IsNullOrEmpty(options.DatabaseName) || string.IsNullOrEmpty(options.ConfigCollectionName))
                {
                    throw new InvalidOperationException("MongoDB connection string, database name, and collection name must be provided when using ConfigHubMongoClient.");
                }


                services.AddSingleton(options);
                services.AddSingleton<IMongoRepositoryFactory, MongoRepositoryFactory>(provider => new MongoRepositoryFactory(options.ConnectionString));
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddScoped<IConfigHubClient, ConfigHubMongoClient>();
            }
        }
    }
}
