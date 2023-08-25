using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Infrastructure.Contract;
using ConfigHub.Mongo;
using ConfigHub.Mongo.Interface;
using ConfigHub.Shared.Entity;
using ConfigHub.Shared.Entity.ConfigHub.Domain.Entity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConfigHub.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IMongoRepository<ConfigItem> configItemRepository;
        private readonly IMongoRepository<ConfigItemHistory> configItemHistoryRepository;
        private readonly IMongoRepository<CertificateMappingDocument> certificateMappingRepo;
        private readonly IMongoRepository<AppInfo> appInfoRepository;
        public ConfigService(IMongoRepositoryFactory mongoRepositoryFactory)
        {
            configItemRepository = mongoRepositoryFactory.GetRepository<ConfigItem>(DBNames.ConfigHubDBName, CollectionName.ConfigCollectionName);
            certificateMappingRepo = mongoRepositoryFactory.GetRepository<CertificateMappingDocument>(DBNames.ConfigHubDBName, CollectionName.ApplicationCertificateInfo);
            configItemHistoryRepository = mongoRepositoryFactory.GetRepository<ConfigItemHistory>(DBNames.ConfigHubDBName, CollectionName.ConfigHistoryCollectionName);
            appInfoRepository = mongoRepositoryFactory.GetRepository<AppInfo>(DBNames.ConfigHubDBName, CollectionName.AppInfoCollectionName);
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
            ValidateLinkedKeyAndValue(configItem.LinkedKey, configItem.Value);
            await ValidateApplicationAndComponentAsync(configItem.ApplicationName, configItem.Component);

            configItem.LastUpdatedDateTime = DateTime.UtcNow;
            configItem.CreatedDateTime = DateTime.UtcNow;
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
            ValidateLinkedKeyAndValue(configItem.LinkedKey, configItem.Value);
            await ValidateApplicationAndComponentAsync(configItem.ApplicationName, configItem.Component);

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
            var filter = Builders<AppInfo>.Filter.Empty;
            return await appInfoRepository.FindAllAsync(filter);
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
            if (!string.IsNullOrWhiteSpace(configItem.LinkedKey))
            {
                var linkedConfigItem = await GetConfigItemByLinkedKeyAsync(configItem.LinkedKey).ConfigureAwait(false);
                if (linkedConfigItem != null)
                {
                    linkedValue = linkedConfigItem.Value;
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
                LastModifiedDateTime = DateTime.UtcNow,
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

        public async Task<ComponentInfo> AddComponentAsync(string applicationName, ComponentInfo component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var filter = Builders<AppInfo>.Filter.Eq(x => x.ApplicationName, applicationName);

            // Fetch the existing AppInfo
            var existingAppInfo = await appInfoRepository.FindOneAsync(filter);


            if (existingAppInfo == null)
            {
                throw new InvalidOperationException("Application not found.");
            }

            if (existingAppInfo.Components == null)
            {
                existingAppInfo.Components = new List<ComponentInfo>();
            }

            if (existingAppInfo.Components.Any(c => c.Name == component.Name) == true)
            {
                throw new InvalidOperationException("Component name already exists.");
            }

            // Assign a new Id to the component
            component.Id = ObjectId.GenerateNewId().ToString();


            // Update the Components list in-memory
            existingAppInfo.Components.Add(component);

            // Update the AppInfo document
            await appInfoRepository.UpdateAsync(existingAppInfo.Id, existingAppInfo);

            // Check if the component was added
            if (existingAppInfo.Components.Contains(component))
            {
                return component;
            }
            else
            {
                throw new InvalidOperationException("Component was not added successfully.");
            }
        }


        public async Task<ComponentInfo> CloneComponentAsync(string applicationName, CloneComponentRequest request)
        {
            var clonedComponent = new ComponentInfo
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = request.TargetComponentName,
            };

            await AddComponentAsync(applicationName, clonedComponent);

            const int pageSize = 500;
            var configItemsResponse = await GetAllConfigItemsByComponent(applicationName, request.SourceComponentName, take: pageSize, skip: 0);
            var totalConfigItemCount = configItemsResponse.totalCount;

            var tasks = new List<Task>();

            for (int skip = 0; skip < totalConfigItemCount; skip += pageSize)
            {
                var configItemsPage = await GetAllConfigItemsByComponent(applicationName, request.SourceComponentName, take: pageSize, skip: skip);

                foreach (var sourceConfigItem in configItemsPage.configItems)
                {
                    var clonedConfigItem = new ConfigItem
                    {
                        ApplicationName = applicationName,
                        Component = request.TargetComponentName,
                        Key = sourceConfigItem.Key,
                        Value = request.CopyValuesFromSource ? sourceConfigItem.Value : request.DefaultValue,
                        LinkedKey = sourceConfigItem.LinkedKey,
                        IsEncrypted = sourceConfigItem.IsEncrypted,
                        LastUpdatedDateTime = DateTime.UtcNow,
                        CreatedDateTime = DateTime.UtcNow
                    };

                    tasks.Add(AddConfigItemAsync(clonedConfigItem));
                }
            }

            await Task.WhenAll(tasks);

            return clonedComponent;
        }



        public async Task<bool> DeleteComponentAsync(string appName, string componentName)
        {
            await ValidateApplicationAndComponentAsync(appName, componentName);

            var appInfo = await appInfoRepository.FindOneAsync(a => a.ApplicationName == appName);

            if (appInfo != null && appInfo.Components != null)
            {
                appInfo.Components.RemoveAll(c => c.Name == componentName);

                await appInfoRepository.UpdateAsync(appInfo.Id, appInfo);

                var updatedAppInfo = await appInfoRepository.FindOneAsync(a => a.ApplicationName == appName);
                if (updatedAppInfo == null || updatedAppInfo.Components.Any(c => c.Name == componentName))
                {
                    return false;
                }

                return true;
            }

            return false;
        }


        public async Task<AppInfo> AddApplicationAsync(AppInfo appInfo)
        {
            if (appInfo == null)
            {
                throw new ArgumentNullException(nameof(appInfo));
            }

            await appInfoRepository.InsertOneAsync(appInfo);

            return appInfo;
        }

        private async Task<ConfigItem> GetConfigItemByLinkedKeyAsync(string formattedLinkedKey)
        {
            var linkedParts = formattedLinkedKey.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (linkedParts.Length == 3)
            {
                var linkedApplicationName = linkedParts[0];
                var linkedComponent = linkedParts[1];
                var linkedKey = linkedParts[2];

                return await GetConfigItemByKeyAndComponent(linkedApplicationName, linkedComponent, linkedKey).ConfigureAwait(false);
            }

            return null;
        }


        private async Task ValidateApplicationAndComponentAsync(string applicationName, string componentName)
        {
            var appInfo = await appInfoRepository.FindOneAsync(a => a.ApplicationName == applicationName);
            if (appInfo == null)
            {
                throw new ArgumentException("Invalid ApplicationName.");
            }

            if (!appInfo.Components.Any(c => c.Name == componentName))
            {
                throw new ArgumentException("Invalid Component.");
            }
        }

        private async void ValidateLinkedKeyAndValue(string linkedKey, string value)
        {
            if (string.IsNullOrWhiteSpace(linkedKey) && string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Either LinkedKey or Value must be provided.");
            }

            if (!string.IsNullOrWhiteSpace(linkedKey) && !string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Both LinkedKey and Value cannot be provided.");
            }

            if (string.IsNullOrWhiteSpace(linkedKey))
            {
                await ValidateLinkedKeyAsync(linkedKey);
            }
        }

        private async Task ValidateLinkedKeyAsync(string linkedKey)
        {
            if (!string.IsNullOrWhiteSpace(linkedKey))
            {
                var linkedConfigItem = await GetConfigItemByLinkedKeyAsync(linkedKey).ConfigureAwait(false);
                if (linkedConfigItem == null)
                {
                    throw new ArgumentException("LinkedKey does not correspond to a valid ConfigItem.");
                }
            }
        }

    }
}
