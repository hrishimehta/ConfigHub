using ConfigHub.Domain.Entity;

namespace ConfigHub.Domain.Interface
{
    public interface IConfigService
    {
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key);
        Task<IEnumerable<ConfigItem>> GetAllConfigItemsByComponent(string applicationId, string componentId);
        Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId);
        Task<ConfigItem> AddConfigItemAsync(ConfigItem configItem);
        Task<IEnumerable<string>> GetAllApplicationNamesAsync();
    }
}
