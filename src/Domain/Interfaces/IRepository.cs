using System.Linq.Expressions;

namespace HotelServices.Domain.Interfaces;

public interface IRepository<T> : IDisposable where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate);

    Task<T> GetByIdAsync(int id);

    Task CreateAsync(T entity);

    Task<bool> UpdateAsync(int id, T entity);

    Task<bool> DeleteAsync(int id);
}