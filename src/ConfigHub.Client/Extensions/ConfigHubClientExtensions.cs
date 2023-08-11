using ConfigHub.Client.Encryption;
using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Services;
using ConfigHub.Mongo.Interface;
using ConfigHub.Mongo.Services;
using ConfigHub.Shared.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;

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
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
            };

            ConventionRegistry.Register("MyConventionPack", conventionPack, t => true);

            services.AddSingleton(options);
            services.AddSingleton<IEncryptor, AesEncryptor>(); // Register IEncryptor implementation
            services.AddSingleton<IMemoryCache, MemoryCache>(); 
            if (options.UseConfigHubService)
            {

                if (string.IsNullOrEmpty(options.BaseAddress) || options.ClientCertificate == null || string.IsNullOrEmpty(options.ApplicationName))
                {
                    throw new InvalidOperationException("ConfigHub base address, client certificate and ApplicaitonId must be provided when using ConfigHubServiceClient.");
                }

                services.AddScoped<IConfigHubClient, ConfigHubServiceClient>();
            }
            else
            {
                if (string.IsNullOrEmpty(options.ConnectionString) || string.IsNullOrEmpty(options.DatabaseName) || string.IsNullOrEmpty(options.ConfigCollectionName))
                {
                    throw new InvalidOperationException("MongoDB connection string, database name, and collection name must be provided when using ConfigHubMongoClient.");
                }
                                
                services.AddSingleton<IMongoRepositoryFactory, MongoRepositoryFactory>(provider => new MongoRepositoryFactory(options.ConnectionString));
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddScoped<IConfigHubClient, ConfigHubMongoClient>();
            }

            LoadAndCacheComponentData(services, options);
        }


        private static void LoadAndCacheComponentData(IServiceCollection services, ConfigHubOptions options)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configService = serviceProvider.GetRequiredService<IConfigHubClient>();

            foreach (var component in options.Components)
            {
                var configItems = configService.GetAllConfigItemsByComponent(component).Result;
                foreach (var configItem in configItems)
                {
                    var cacheKey = $"{component}_{configItem.Key}";
                    serviceProvider.GetRequiredService<IMemoryCache>().Set(cacheKey, configItem);
                }
            }
        }
    }
}
