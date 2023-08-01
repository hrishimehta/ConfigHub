using ConfigHub.Domain.Entity;
using ConfigHub.Infrastructure.Contract;
using ConfigHub.Infrastructure.Interface;
using ConfigHub.Mongo;
using ConfigHub.Mongo.Interface;
using MongoDB.Driver;

namespace ConfigHub.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IMongoRepository<ConfigItem> _configItemRepository;
        private readonly IMongoRepository<CertificateMappingDocument> certificateMappingRepo;

        public ConfigService(IMongoRepositoryFactory mongoRepositoryFactory)
        {
            _configItemRepository = mongoRepositoryFactory.GetRepository<ConfigItem>(DBNames.ConfigHubDBName, CollectionName.ConfigCollectionName);
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key)
        {
            var filter = Builders<ConfigItem>.Filter.Eq("Component", componentId) &
                    Builders<ConfigItem>.Filter.Eq("ApplicationName", applicationId) &
                    Builders<ConfigItem>.Filter.Eq("Key", key) ;

            var configItem = await _configItemRepository.FindAllAsync(filter);

            return configItem.FirstOrDefault();
        }

        public async Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId)
        {
            var filter = Builders<CertificateMappingDocument>.Filter.Eq("Thumbprint", thumbprint) &
                     Builders<CertificateMappingDocument>.Filter.Eq("ApplicationId", applicationId);

            long count = await this.certificateMappingRepo.CountAsync(filter);

            return count > 0;
        }

        public async Task<IEnumerable<ConfigItem>> GetAllConfigItemsByComponent(string applicationId, string componentId)
        {
            var configItems = await _configItemRepository.FindAllAsync(c => c.ApplicationName == applicationId &&
                                                                 c.Component == componentId);

            return configItems;
        }

    }

}
