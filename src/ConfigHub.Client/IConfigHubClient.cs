using ConfigHub.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Client
{
    public interface IConfigHubClient
    {
        Task<ConfigItem> GetConfigItemByKeyAndComponent(string component, string key);
        Task<List<ConfigItem>> GetAllConfigItemsByComponent(string component);
    }
}
