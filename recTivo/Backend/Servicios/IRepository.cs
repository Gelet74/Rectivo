using System.Collections.Generic;
using System.Threading.Tasks;

namespace recTivo.Backend.Repos;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();

    Task UpdateAsync (T entity);

    Task DeleteAsync (int id);

    Task AddAsync (T[] entities);
    Task DeleteAsync(object entity);
}