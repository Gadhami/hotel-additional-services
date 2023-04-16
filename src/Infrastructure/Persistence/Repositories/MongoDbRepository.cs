using System.Linq.Expressions;
using HotelServices.Domain.Interfaces;

using HotelServices.Infrastructure.Persistence.Models;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HotelServices.Infrastructure.Persistence.Repositories;

public class MongoDbRepository<T> : IRepository<T>, IDisposable where T : class
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<T> _collection;
    private readonly Expression<Func<T, int>> _idExpression;
    private bool _disposed = false;

    public MongoDbRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> options)
    {
        var database  = mongoClient.GetDatabase(options.Value.DatabaseName);
        _client       = mongoClient;
        _collection   = database.GetCollection<T>(typeof(T).Name + "s");

        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null || idProperty.PropertyType != typeof(int))
        {
            throw new ArgumentException($"Type {typeof(T).FullName} must have an 'Id' property of type 'int'.");
        }

        var param     = Expression.Parameter(typeof(T), "x");
        _idExpression = Expression.Lambda<Func<T, int>>(Expression.Property(param, idProperty), param);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var entities = await _collection.FindAsync(Builders<T>.Filter.Empty);
        return await entities.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate)
    {
        var filter = Builders<T>.Filter.Empty;
        if (predicate != null)
        {
            filter = Builders<T>.Filter.Where(predicate);
        }
        var entities = await _collection.FindAsync(filter);
        return await entities.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var filter = Builders<T>.Filter.Eq(_idExpression, id);
        var entity = await _collection.FindAsync(filter);

        return await entity.FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task<bool> UpdateAsync(int id, T entity)
    {
        var filter = Builders<T>.Filter.Eq(_idExpression, id);
        var result = await _collection.ReplaceOneAsync(filter, entity);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var filter = Builders<T>.Filter.Eq(_idExpression, id);
        var result = await _collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                (_client as IDisposable)?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}