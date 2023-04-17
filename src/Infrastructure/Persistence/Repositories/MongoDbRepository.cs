using System.Collections;
using System.Linq.Expressions;

using HotelServices.Domain.Interfaces;
using HotelServices.Infrastructure.Persistence.Configuration;

using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;

namespace HotelServices.Infrastructure.Persistence.Repositories;

public class MongoDbRepository<T> : IRepository<T>, IDisposable where T : class, IEntity
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<T> _collection;
    private bool _disposed = false;

    public MongoDbRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _client      = mongoClient;
        _collection  = database.GetCollection<T>(typeof(T).Name + "s");
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

    public async Task<T> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var entity = await _collection.FindAsync(filter);

        return await entity.FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
        }

        AddMissingChildEntityIDs(entity);

        await _collection.InsertOneAsync(entity);
    }

    private static void AddMissingChildEntityIDs(T entity)
    {
        // check for child entities
        foreach (var property in typeof(T).GetProperties())
        {
            var type = property.PropertyType;

            // check if the property is a collection of child entities
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var childEntities = (IEnumerable)property.GetValue(entity);

                foreach (var childEntity in childEntities)
                {
                    var childEntityType = childEntity.GetType();

                    if (childEntityType.GetInterfaces().Contains(typeof(IEntity)))
                    {
                        var childEntityId = (string)childEntityType.GetProperty("Id").GetValue(childEntity);

                        if (string.IsNullOrEmpty(childEntityId))
                        {
                            childEntityType.GetProperty("Id").SetValue(childEntity, ObjectId.GenerateNewId().ToString());
                        }
                    }
                }
            }
        }
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
        }

        AddMissingChildEntityIDs(entity);

        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var result = await _collection.ReplaceOneAsync(filter, entity);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
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