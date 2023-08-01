using ConfigHub.Mongo.Interface;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Mongo.Services
{
    public class MongoRepositoryFactory : IMongoRepositoryFactory 
    {
        private readonly IMongoClient _mongoClient;
        private readonly ConcurrentDictionary<string, object> _repositories;

        public MongoRepositoryFactory(string connectionString)
        {
            _mongoClient = new MongoClient(connectionString);
            _repositories = new ConcurrentDictionary<string, object>();
        }

        public IMongoRepository<T> GetRepository<T>(string dbName, string collectionName) where T : class
        {
            var key = $"{typeof(T).FullName}_{dbName}_{collectionName}";
            return (IMongoRepository<T>)_repositories.GetOrAdd(key, _ =>
            {
                var database = _mongoClient.GetDatabase(dbName);
                return new GenericMongoRepository<T>(database.GetCollection<T>(collectionName));
            });
        }
    }
}
