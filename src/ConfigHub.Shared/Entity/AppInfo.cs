using ConfigHub.Domain.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace ConfigHub.Shared.Entity
{
    public class AppInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ApplicationName { get; set; }

        public List<ComponentInfo> Components { get; set; }

        
    }
}
