using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConfigHub.Infrastructure.Contract
{
    [Serializable]
    public class CertificateMappingDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Thumbprint")]
        public string Thumbprint { get; set; }

        [BsonElement("ApplicationName")]
        public string ApplicationName { get; set; }
    }
}
