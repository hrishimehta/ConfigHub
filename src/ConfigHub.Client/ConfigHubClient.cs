using ConfigHub.Domain.Entity;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace ConfigHub.Client
{
    public class ConfigHubClient : IConfigHubClient
    {
        private readonly HttpClient _httpClient;

        public ConfigHubClient(string baseAddress, X509Certificate2 clientCertificate, string applicationId)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            if (clientCertificate != null)
            {
                _httpClient.DefaultRequestHeaders.Add("X-Client-Cert", Convert.ToBase64String(clientCertificate.RawData));
            }

            _httpClient.DefaultRequestHeaders.Add("X-ApplicationId", applicationId);
        }

        public async Task<ConfigItem> GetConfigItemByKeyAndComponent(string component, string key)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Config/component/{component}/key/{key}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ConfigItem>(json);
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
