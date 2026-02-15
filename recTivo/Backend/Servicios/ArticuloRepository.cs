using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos
{
    public class ArticuloRepository : GenericRepository<Articulo>, IArticuloRepository
    {
        public ArticuloRepository(RectivoContext context) : base(context) { }

        public async Task<Articulo?> GetByCodigoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;

            codigo = codigo.Trim().ToUpper();

            return await _dbSet
                 .Include(a => a.ArticuloUbicaciones) // Carga las ubicaciones para que UbicacionesResumen no de "-"
                 .FirstOrDefaultAsync(a => a.Codigo.Trim().ToUpper() == codigo);
        }

        public async Task<IEnumerable<Articulo>> GetByUbicacionAsync(int idUbicacion)
        {
            // Importante: Usamos _context.Ubicacion porque así está en tu RectivoContext
            return await _context.Ubicacion
                .Where(u => u.IdUbicacion == idUbicacion && u.Articulo != null)
                .Select(u => u.Articulo!)
                .ToListAsync();
        }

        public async Task<Articulo?> GetWithRelationsAsync(int id)
        {
            return await _dbSet
                .Include(a => a.ClienteHasArticulos)
                .Include(a => a.ArticuloUbicaciones) // Relación 1:N directa, ya no hay tabla intermedia
                .FirstOrDefaultAsync(a => a.IdArticulo == id);
        }
    }
}