using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace recTivo.Backend.Repos
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<GenericRepository<T>>? _logger;

        // Constructor sin logger
        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Constructor con logger
        public GenericRepository(DbContext context, ILogger<GenericRepository<T>> logger)
            : this(context)
        {
            _logger = logger;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            _logger?.LogInformation("Obteniendo todos los registros de {Entity}", typeof(T).Name);
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            _logger?.LogInformation("Buscando {Entity} por clave: {Keys}", typeof(T).Name, keyValues);
            return await _dbSet.FindAsync(keyValues);
        }

        public virtual async Task AddAsync(T entity)
        {
            _logger?.LogInformation("Agregando nuevo {Entity}", typeof(T).Name);
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            _logger?.LogInformation("Actualizando {Entity}", typeof(T).Name);
            _dbSet.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            _logger?.LogInformation("Eliminando {Entity}", typeof(T).Name);
            _dbSet.Remove(entity);
        }

        public virtual Task<int> SaveChangesAsync()
        {
            _logger?.LogInformation("Guardando cambios en {Entity}", typeof(T).Name);
            return _context.SaveChangesAsync();
        }
    }
}
