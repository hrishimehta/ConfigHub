using ConfigHub.Domain.Entity;

namespace ConfigHub.Client
{
    public interface IConfigHubClient
    {
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string component, string key, int take, int skip);
        Task<List<ConfigItem>> GetAllConfigItemsByComponent(string component);
    }
}
