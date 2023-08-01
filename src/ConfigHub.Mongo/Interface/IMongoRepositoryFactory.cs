using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Mongo.Interface
{
    public interface IMongoRepositoryFactory
    {
        IMongoRepository<T> GetRepository<T>(string dbName, string collectionName) where T : class;
    }

}
