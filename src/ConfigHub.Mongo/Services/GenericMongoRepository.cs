using ConfigHub.Mongo.Interface;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace ConfigHub.Mongo.Services
{
    public class GenericMongoRepository<T> : IMongoRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public GenericMongoRepository(IMongoCollection<T> _collection)
        {
            this._collection = _collection;
        }

        public async Task<T> FindByIdAsync(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> filter = null)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public Task<List<T>> FindAllAsync(Expression<Func<T, bool>> filter, ProjectionDefinition<T, T> projection)
        {
            return _collection.Find(filter).Project(projection).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(FilterDefinition<T> filter, int take, int skip)
        {
            var options = new FindOptions<T>
            {
                Limit = take,
                Skip = skip
            };

            using (var cursor = await _collection.FindAsync(filter, options))
            {
                return await cursor.ToListAsync();
            }
        }

        public async Task<IEnumerable<T>> FindAllAsync(FilterDefinition<T> filter = null)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(FilterDefinition<T> filter, ProjectionDefinition<T, T> projection)
        {
            return await _collection.Find(filter).Project(projection).ToListAsync();
        }

        public async Task<T> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            if (filter == null)
                return await _collection.EstimatedDocumentCountAsync();

            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task<long> CountAsync(FilterDefinition<T> filter = null)
        {
            if (filter == null)
                return await _collection.EstimatedDocumentCountAsync();

            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task InsertOneAsync(T document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            await _collection.InsertOneAsync(document);
        }

        public async Task InsertManyAsync(IEnumerable<T> documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

            await _collection.InsertManyAsync(documents);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            ObjectId objectId = ObjectId.Parse(id);
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
        public async Task<T> FindOneAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

    }
}
