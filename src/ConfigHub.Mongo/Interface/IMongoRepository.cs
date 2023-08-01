﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Mongo.Interface
{
    public interface IMongoRepository<T>
    {
        Task<T> FindByIdAsync(string id);
        Task<IEnumerable<T>> FindAllAsync(FilterDefinition<T> filter = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> filter = null);
        Task<IEnumerable<TProjection>> FindAllAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection);
        Task<T> FindOneAsync(Expression<Func<T, bool>> filter);
        Task<long> CountAsync(Expression<Func<T, bool>> filter = null);
        Task<long> CountAsync(FilterDefinition<T> filter = null);
    }
}