using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Infrastructure.Contract
{
    public class CertificateMappingDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Thumbprint")]
        public string Thumbprint { get; set; }

        [BsonElement("ApplicationId")]
        public int ApplicationId { get; set; }
    }
}
