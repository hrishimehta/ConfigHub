using ConfigHub.Mongo.Interface;
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
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> filter = null)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(FilterDefinition<T> filter = null)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<TProjection>> FindAllAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection)
        {
            var query = _collection.AsQueryable().OfType<T>();

            if (filter != null)
                query = query.Where(filter);

            return await query.Select(projection).ToListAsync();
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
    }
}
