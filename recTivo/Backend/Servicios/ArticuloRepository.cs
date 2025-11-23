using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace recTivo.Backend.Repos
{
    public class ArticuloRepository : GenericRepository<Articulo>, IArticuloRepository
    {
        public ArticuloRepository(DbContext context) : base(context) { }

        // Buscar por código
        public async Task<Articulo?> GetByCodigoAsync(string codigo)
            => await _dbSet.FirstOrDefaultAsync(a => a.Codigo == codigo);

        // Obtener todos los artículos por ID de ubicación
        public async Task<IEnumerable<Articulo>> GetByUbicacionAsync(int idUbicacion)
            => await _dbSet.Where(a => a.IdUbicacion == idUbicacion).ToListAsync();

        // Obtener artículos con sus relaciones cargadas (Ubicacion y ClienteHasArticulo)
        public async Task<Articulo?> GetWithRelationsAsync(int id)
            => await _dbSet
                .Include(a => a.Ubicacion)
                .Include(a => a.ClienteHasArticulos)
                .FirstOrDefaultAsync(a => a.IdArticulo == id);
    }
}
