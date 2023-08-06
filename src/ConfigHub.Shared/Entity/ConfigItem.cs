using System;

namespace ConfigHub.Domain.Entity
{
    [Serializable]
    public class ConfigItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string HashedValue { get; set; }
        public bool IsEncrypted { get; set; }
        public string Component { get; set; }
        public string ApplicationName { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string LastUpdatedBy { get; set; }
        public int MemCacheDurationInMinute { get; set; }
    }
}
