using System.Security.Cryptography.X509Certificates;

namespace ConfigHub.Shared.Options
{
    public class ConfigHubOptions
    {
        public bool UseConfigHubService { get; set; } = true;
        public string BaseAddress { get; set; } // ConfigHub Service url
        public string ApplicationName { get; set; } // applicaiton name
        public List<string> Components { get; set; } = new List<string>();//  list of components to load and cache at initialization
        public string ConnectionString { get; set; } // MongoDB connection string
        public string DatabaseName { get; set; } // MongoDB database name
        public string ConfigCollectionName { get; set; } // MongoDB collection name for configuration data
        public string CertificateMappingCollectionName { get; set; } // MongoDB collection name for certificate mappings
        public X509Certificate2 ClientCertificate { get; set; } // Client certificate for connecting to the service

        public string EncryptionKey { get;set; } // Encryption key
    }
}
