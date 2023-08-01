using ConfigHub.Domain.Entity;
using System.Security.Cryptography.X509Certificates;

namespace ConfigHub.Infrastructure.Interface
{
    public interface IConfigService
    {
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string applicationId, string componentId, string key);
        Task<IEnumerable<ConfigItem>> GetAllConfigItemsByComponent(string applicationId, string componentId);
        Task<bool> IsValidApplicationCertificateMappingAsync(string thumbprint, string applicationId);
    }
}
