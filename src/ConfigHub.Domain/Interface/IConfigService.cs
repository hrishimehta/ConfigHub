using ConfigHub.Domain.Entity;
using ConfigHub.Shared.Entity;

namespace ConfigHub.Domain.Interface
{
    public interface IConfigService
    {
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key);
        Task<IEnumerable<ConfigItem>> GetAllConfigItemsByComponent(string applicationId, string componentId);
        Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId);
        Task<ConfigItem> AddConfigItemAsync(ConfigItem configItem);

        Task<ConfigItem> UpdateConfigItemAsync(ConfigItem configItem);

        Task<IEnumerable<string>> GetAllApplicationNamesAsync();

        Task<IEnumerable<AppInfo>> GetAllAppInfoAsync();

        Task<IEnumerable<ConfigItem>> SearchConfigItems(string search, int take, int skip);


    }
}
