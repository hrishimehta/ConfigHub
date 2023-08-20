using ConfigHub.Domain.Entity;
using ConfigHub.Domain.Interface;
using ConfigHub.Shared.Options;

namespace ConfigHub.Client
{
    public class ConfigHubMongoClient : IConfigHubClient
    {
        private readonly IConfigService _configService;

        private readonly ConfigHubOptions _configHubOptions;

        public ConfigHubMongoClient(IConfigService configService, ConfigHubOptions configHubOptions)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _configHubOptions = configHubOptions ?? throw new ArgumentNullException(nameof(configHubOptions));
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string component, string key)
        {
            return await _configService.GetConfigItemByKeyAndComponent(_configHubOptions.ApplicationName, component, key);
        }

        public async Task<List<ConfigItem>> GetAllConfigItemsByComponent(string component, int take, int skip)
        {
            return (await _configService.GetAllConfigItemsByComponent(_configHubOptions.ApplicationName, component, take, skip).ConfigureAwait(false)).ToList();
        }
    }
}
