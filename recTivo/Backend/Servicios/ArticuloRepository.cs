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
                 .Include(a => a.Ubicaciones)
                 .FirstOrDefaultAsync(a => a.Codigo.Trim().ToUpper() == codigo);
        }

        public async Task<Articulo?> GetWithRelationsAsync(int id)
        {
            return await _dbSet
                .Include(a => a.ClienteHasArticulos)
                .Include(a => a.Ubicaciones) 
                .FirstOrDefaultAsync(a => a.IdArticulo == id);
        }
    }
}