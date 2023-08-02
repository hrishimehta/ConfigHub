using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace ConfigHub.Client
{
    public static class ConfigHubClientExtensions
    {
        public static void AddConfigHubClient(this IServiceCollection services, string baseAddress, X509Certificate2 clientCertificate, string applicationId)
        {
            services.AddHttpClient<IConfigHubClient, ConfigHubClient>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Add("X-ApplicationId", applicationId);

                    if (clientCertificate != null)
                    {
                        client.DefaultRequestHeaders.Add("X-Client-Cert", Convert.ToBase64String(clientCertificate.RawData));
                    }
                });
        }
    }
}
