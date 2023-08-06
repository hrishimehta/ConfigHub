using ConfigHub.Domain.Entity;
using ConfigHub.Shared;
using ConfigHub.Shared.Options;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace ConfigHub.Client
{
    public class ConfigHubServiceClient : IConfigHubClient
    {
        private readonly HttpClient _httpClient;

        public ConfigHubServiceClient(ConfigHubOptions configHubOptions)
        {
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
                return JsonSerializer.Deserialize<ConfigItem>(json, jsonSerializerOptions);
            }
            catch (HttpRequestException)
            {
                // Handle error
                throw;
            }
        }

        public async Task<List<ConfigItem>> GetAllConfigItemsByComponent(string component)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Config/component/{component}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ConfigItem>>(json);
            }
            catch (HttpRequestException)
            {
                // Handle error
                throw;
            }
        }
    }
}
