using ConfigHub.Client.Encryption;
using ConfigHub.Domain.Entity;
using ConfigHub.Shared;
using ConfigHub.Shared.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ConfigHub.Client
{
    public class ConfigHubServiceClient : IConfigHubClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConfigHubOptions _configHubOptions;
        private readonly IEncryptor _encryptor;
        private readonly HttpClient _httpClient;

        public ConfigHubServiceClient(ConfigHubOptions configHubOptions, IMemoryCache memoryCache, IEncryptor encryptor)
        {
            _memoryCache = memoryCache;
            _configHubOptions = configHubOptions;
            _encryptor = encryptor;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configHubOptions.BaseAddress)
            };

            if (configHubOptions.ClientCertificate != null)
            {
                _httpClient.DefaultRequestHeaders.Add(Constants.ClientCertificateHeader, Convert.ToBase64String(configHubOptions.ClientCertificate.RawData));
            }

            _httpClient.DefaultRequestHeaders.Add(Constants.ApplicationNameHeader, configHubOptions.ApplicationName);
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string component, string key)
        {
            var cacheKey = $"{component}_{key}";
            var cachedConfigItem = _memoryCache.Get<ConfigItem>(cacheKey);
            if (cachedConfigItem != null)
            {
                return cachedConfigItem;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Config/component/{component}/key/{key}");
                response.EnsureSuccessStatusCode();

                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = await response.Content.ReadAsStringAsync();
                var configItem = JsonSerializer.Deserialize<ConfigItem>(json, jsonSerializerOptions);

                if (configItem != null && configItem.IsEncrypted)
                {
                    configItem.Value = _encryptor.Decrypt(configItem.HashedValue, _configHubOptions.EncryptionKey);
                }

                // Cache the fetched config item
                _memoryCache.Set(cacheKey, configItem);

                return configItem;
            }
            catch (HttpRequestException)
            {
                // Handle error
                throw;
            }
        }

        public async Task<List<ConfigItem>> GetAllConfigItemsByComponent(string component)
        {
            var response = await _httpClient.GetAsync($"api/Config/component/{component}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var configItems = JsonSerializer.Deserialize<List<ConfigItem>>(json);

            if (configItems != null)
            {
                foreach (var configItem in configItems)
                {
                    if (configItem.IsEncrypted)
                    {
                        configItem.Value = _encryptor.Decrypt(configItem.HashedValue, _configHubOptions.EncryptionKey);
                    }

                    // Cache each config item
                    var cacheKey = $"{component}_{configItem.Key}";
                    _memoryCache.Set(cacheKey, configItem);
                }
            }

            return configItems;
        }
    }
}
