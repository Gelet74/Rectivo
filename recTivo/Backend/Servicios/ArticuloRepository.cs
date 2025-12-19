using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos
{
    public class ArticuloRepository : GenericRepository<Articulo>, IArticuloRepository
    {
        public ArticuloRepository(RectivoContext context) : base(context) { }


        // Buscar por código
        public async Task<Articulo?> GetByCodigoAsync(string codigo)
        {
            codigo = codigo.Trim().ToUpper();

            return await _dbSet
                 .FirstOrDefaultAsync(a => a.Codigo.Trim().ToUpper() == codigo);
        }



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
