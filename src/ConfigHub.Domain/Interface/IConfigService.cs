using ConfigHub.Domain.Entity;
using ConfigHub.Shared.Entity.ConfigHub.Domain.Entity;
using ConfigHub.Shared.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigHub.Domain.Interface
{
    public interface IConfigService
    {
        Task<ConfigItem> GetConfigItemByIdAsync(string id);
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key);
        Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId);
        Task<ConfigItem> AddConfigItemAsync(ConfigItem configItem);
        Task DeleteConfigItemAsync(string id);
        Task<ConfigItem> UpdateConfigItemAsync(ConfigItem configItem);
        Task<IEnumerable<string>> GetAllApplicationNamesAsync();
        Task<IEnumerable<AppInfo>> GetAllAppInfoAsync();
        Task<(IEnumerable<ConfigItem> configItems, long totalCount)> GetAllConfigItemsByComponent(string applicationId, string componentId, int take, int skip);
        Task<(IEnumerable<ConfigItem> configItems, long totalCount)> SearchConfigItems(string search, int take, int skip);
        Task<string> GetLinkedValue(ConfigItem configItem);
        Task<(IEnumerable<ConfigItemHistory> historyItems, long totalCount)> GetConfigItemHistoryByIdAsync(string id, int take, int skip);
        Task<(IEnumerable<ConfigItemHistory> historyItems, long totalCount)> GetConfigItemHistoryByOperationAsync(string applicationId, string componentId, OperationType? operationType, int take, int skip);
    }
}
