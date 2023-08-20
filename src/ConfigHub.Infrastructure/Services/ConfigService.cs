using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Contract;
using ConfigHub.Mongo;
using ConfigHub.Mongo.Interface;
using ConfigHub.Shared.Entity;
using ConfigHub.Shared.Entity.ConfigHub.Domain.Entity;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigHub.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IMongoRepository<ConfigItem> configItemRepository;
        private readonly IMongoRepository<ConfigItemHistory> configItemHistoryRepository;
        private readonly IMongoRepository<CertificateMappingDocument> certificateMappingRepo;

        public ConfigService(IMongoRepositoryFactory mongoRepositoryFactory)
        {
            configItemRepository = mongoRepositoryFactory.GetRepository<ConfigItem>(DBNames.ConfigHubDBName, CollectionName.ConfigCollectionName);
            certificateMappingRepo = mongoRepositoryFactory.GetRepository<CertificateMappingDocument>(DBNames.ConfigHubDBName, CollectionName.ApplicationCertificateInfo);
            configItemHistoryRepository = mongoRepositoryFactory.GetRepository<ConfigItemHistory>(DBNames.ConfigHubDBName, CollectionName.ConfigHistoryCollectionName);
        }

        public async Task<ConfigItem> GetConfigItemByIdAsync(string id)
        {
            return await configItemRepository.FindByIdAsync(id);
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key)
        {
            var filter = Builders<ConfigItem>.Filter.Eq("Component", componentId) &
                         Builders<ConfigItem>.Filter.Eq("ApplicationName", applicationId) &
                         Builders<ConfigItem>.Filter.Eq("Key", key);

            var configItem = (await configItemRepository.FindAllAsync(filter)).FirstOrDefault();
            configItem.Value = await GetLinkedValue(configItem);

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
            await CreateHistoryEntry(configItem, OperationType.Create, configItem.LastUpdatedBy, new List<string>());
            return configItem;
        }

        public async Task DeleteConfigItemAsync(string id)
        {
            var existingConfig = await configItemRepository.FindByIdAsync(id);

            if (existingConfig != null)
            {
                await configItemRepository.DeleteByIdAsync(id);
                await CreateHistoryEntry(existingConfig, OperationType.Delete, string.Empty, new List<string>());
            }
        }

        public async Task<ConfigItem> UpdateConfigItemAsync(ConfigItem configItem)
        {
            var existingConfig = await GetConfigItemByKeyAndComponent(configItem.ApplicationName, configItem.Component, configItem.Key);
            var changedProperties = FindChangedProperties(existingConfig, configItem);

            configItem.LastUpdatedDateTime = DateTime.UtcNow;
            await configItemRepository.UpdateAsync(existingConfig.Id, configItem);
            await CreateHistoryEntry(configItem, OperationType.Update, configItem.LastUpdatedBy, changedProperties);

            return configItem;
        }

        public async Task<IEnumerable<string>> GetAllApplicationNamesAsync()
        {
            var filter = Builders<CertificateMappingDocument>.Filter.Empty;
            var projection = Builders<CertificateMappingDocument>.Projection.Include(doc => doc.ApplicationName);
            var applicationNames = await certificateMappingRepo.FindAllAsync(x => true, projection);
            return applicationNames.Select(item => item.ApplicationName).Distinct();
        }

        public async Task<IEnumerable<AppInfo>> GetAllAppInfoAsync()
        {
            var filter = Builders<ConfigItem>.Filter.Empty;
            var projection = Builders<ConfigItem>.Projection.Include(item => item.ApplicationName).Include(item => item.Component);
            var configItems = await configItemRepository.FindAllAsync(filter, projection);

            var uniqueAppInfoList = configItems
                .GroupBy(item => new { item.ApplicationName })
                .Select(group => new AppInfo { ApplicationName = group.Key.ApplicationName, Components = group.Select(item => item.Component).Distinct().ToList() })
                .Distinct();

            return uniqueAppInfoList;
        }

        public async Task<(IEnumerable<ConfigItem> configItems, long totalCount)> GetAllConfigItemsByComponent(string applicationId, string componentId, int take, int skip)
        {
            var filter = Builders<ConfigItem>.Filter.Eq("ApplicationName", applicationId) &
                         Builders<ConfigItem>.Filter.Eq("Component", componentId);

            var totalCount = await configItemRepository.CountAsync(filter);
            var configItems = await configItemRepository.FindAllAsync(filter, take, skip);

            return (configItems, totalCount);
        }

        public async Task<(IEnumerable<ConfigItem> configItems, long totalCount)> SearchConfigItems(string search, int take, int skip)
        {
            var filter = Builders<ConfigItem>.Filter.Regex("Value", new BsonRegularExpression(search, "i"));
            var totalCount = await configItemRepository.CountAsync(filter);
            var configItems = await configItemRepository.FindAllAsync(filter, take, skip);

            return (configItems, totalCount);
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

        private async Task CreateHistoryEntry(ConfigItem configItem, OperationType operationType, string changedBy, List<string> changedProperties)
        {
            var historyEntry = new ConfigItemHistory
            {
                ItemId = configItem.Id,
                Key = configItem.Key,
                ApplicationName = configItem.ApplicationName, // Add ApplicationName to history entry
                Component = configItem.Component, // Add Component to history entry
                OperationType = operationType,
                ChangedBy = changedBy,
                ChangeDate = DateTime.UtcNow,
                ChangedProperties = changedProperties
            };

            await configItemHistoryRepository.InsertOneAsync(historyEntry);
        }

        private List<string> FindChangedProperties(ConfigItem existingConfig, ConfigItem newConfig)
        {
            var changes = new List<string>();

            if (existingConfig.IsEncrypted != newConfig.IsEncrypted)
            {
                changes.Add($"{nameof(ConfigItem.IsEncrypted)} changed from {existingConfig.IsEncrypted} to {newConfig.IsEncrypted}");
            }

            if (existingConfig.Value != newConfig.Value)
            {
                changes.Add($"{nameof(ConfigItem.Value)} changed from '{existingConfig.Value}' to '{newConfig.Value}'");
            }

            if (existingConfig.LinkedKey != newConfig.LinkedKey)
            {
                changes.Add($"{nameof(ConfigItem.LinkedKey)} changed from '{existingConfig.LinkedKey}' to '{newConfig.LinkedKey}'");
            }

            if (existingConfig.LastUpdatedBy != newConfig.LastUpdatedBy)
            {
                changes.Add($"{nameof(ConfigItem.LastUpdatedBy)} changed from '{existingConfig.LastUpdatedBy}' to '{newConfig.LastUpdatedBy}'");
            }

            return changes;
        }

        public async Task<(IEnumerable<ConfigItemHistory> historyItems, long totalCount)> GetConfigItemHistoryByIdAsync(string id, int take, int skip)
        {
            var filter = Builders<ConfigItemHistory>.Filter.Eq("ItemId", id);
            var totalCount = await configItemHistoryRepository.CountAsync(filter);
            var historyItems = await configItemHistoryRepository.FindAllAsync(filter, take, skip);
            return (historyItems, totalCount);
        }


        public async Task<(IEnumerable<ConfigItemHistory> historyItems, long totalCount)> GetConfigItemHistoryByComponentAsync(string applicationId, string componentId, OperationType? operationType, int take, int skip)
        {
            var filter = Builders<ConfigItemHistory>.Filter.Eq("ApplicationName", applicationId) &
                         Builders<ConfigItemHistory>.Filter.Eq("Component", componentId);

            if (operationType.HasValue)
            {
                filter &= Builders<ConfigItemHistory>.Filter.Eq("OperationType", operationType);
            }

            var totalCount = await configItemHistoryRepository.CountAsync(filter);
            var historyItems = await configItemHistoryRepository.FindAllAsync(filter, take, skip);

            return (historyItems, totalCount);
        }

    }
}
