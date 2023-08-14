﻿using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Contract;
using ConfigHub.Mongo;
using ConfigHub.Mongo.Interface;
using ConfigHub.Shared;
using MongoDB.Driver;
using System.Reflection.Metadata;

namespace ConfigHub.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IMongoRepository<ConfigItem> configItemRepository;
        private readonly IMongoRepository<CertificateMappingDocument> certificateMappingRepo;

        public ConfigService(IMongoRepositoryFactory mongoRepositoryFactory)
        {
            configItemRepository = mongoRepositoryFactory.GetRepository<ConfigItem>(DBNames.ConfigHubDBName, CollectionName.ConfigCollectionName);
            certificateMappingRepo = mongoRepositoryFactory.GetRepository<CertificateMappingDocument>(DBNames.ConfigHubDBName, CollectionName.ApplicationCertificateInfo);
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key)
        {
            var filter = Builders<ConfigItem>.Filter.Eq("Component", componentId) &
                    Builders<ConfigItem>.Filter.Eq("ApplicationName", applicationId) &
                    Builders<ConfigItem>.Filter.Eq("Key", key);

            var configItem = (await configItemRepository.FindAllAsync(filter)).FirstOrDefault();
            configItem.Value = await this.GetLinkedValue(configItem);

            return configItem;
        }

        public async Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId)
        {
            var filter = Builders<CertificateMappingDocument>.Filter.Eq(x => x.Thumbprint, thumbprint) &
                     Builders<CertificateMappingDocument>.Filter.Eq(x => x.ApplicationName, applicationId);

            long count = await this.certificateMappingRepo.CountAsync(filter);

            return count > 0;
        }


        public async Task<ConfigItem> AddConfigItemAsync(ConfigItem configItem)
        {
            await configItemRepository.InsertOneAsync(configItem);
            return configItem;
        }

        public async Task<IEnumerable<string>> GetAllApplicationNamesAsync()
        {
            var filter = Builders<CertificateMappingDocument>.Filter.Empty;
            var projection = Builders<CertificateMappingDocument>.Projection.Include(doc => doc.ApplicationName);

            var applicationNames = await certificateMappingRepo.FindAllAsync(x => true, projection);


            return applicationNames.Select(item => item.ApplicationName).Distinct();
        }


        public async Task<IEnumerable<ConfigItem>> GetAllConfigItemsByComponent(string applicationId, string componentId)
        {

            var configItems = await configItemRepository.FindAllAsync(c => c.ApplicationName == applicationId &&
                                                                 c.Component == componentId);

            foreach (var item in configItems)
            {
                item.Value = await this.GetLinkedValue(item);
            }

            return configItems;
        }

        public async Task<string> GetLinkedValue(ConfigItem configItem)
        {
            string linkedValue = configItem.Value;
            if (configItem.LinkedKey != null)
            {
                var linkedParts = configItem.LinkedKey.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (linkedParts.Length == 3)
                {
                    var linkedApplicationName = linkedParts[0];
                    var linkedComponent = linkedParts[1];
                    var linkedKey = linkedParts[2];

                    linkedValue = (await GetConfigItemByKeyAndComponent(linkedApplicationName, linkedComponent, linkedKey).ConfigureAwait(false)).Value;
                }
            }

            return linkedValue;
        }

    }

}
