using System.Linq.Expressions;
using HotelServices.Domain.Interfaces;
using MongoDB.Driver.Linq;

namespace HotelServices.WebAPI.IntegrationTests.Services;

public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _entities;

    public InMemoryRepository()
    {
        _entities = new List<T>();
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_entities);
    }

    public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate)
    {
        if (predicate == null)
        {
            return GetAllAsync();
        }

        var entities = _entities.AsQueryable().Where(predicate);
        return Task.FromResult<IEnumerable<T>>(entities);
    }

    public Task<T> GetByIdAsync(string id)
    {
        var entity = _entities.FirstOrDefault(e => GetId(e) == id);
        return Task.FromResult(entity);
    }

    public async Task CreateAsync(T entity)
    {
        _entities.Add(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        var existingEntity = _entities.FirstOrDefault(e => GetId(e) == id);
        if (existingEntity == null)
        {
            return false;
        }

        _entities.Remove(existingEntity);
        _entities.Add(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existingEntity = _entities.FirstOrDefault(e => GetId(e) == id);
        if (existingEntity == null)
        {
            return false;
        }

        _entities.Remove(existingEntity);
        return true;
    }

    public void Dispose()
    {
        // no-op
    }

    private static string GetId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
        {
            throw new InvalidOperationException($"Type {typeof(T).FullName} does not have an 'Id' property.");
        }

        var id = idProperty.GetValue(entity);
        if (id == null)
        {
            throw new InvalidOperationException($"Entity of type {typeof(T).FullName} has null Id.");
        }

        return (string)id;
    }
}