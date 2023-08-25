using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ConfigHub.Domain.Entity
{
    public class ComponentInfo
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
