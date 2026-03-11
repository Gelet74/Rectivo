using recTivo.Backend.Modelos;
using recTivo.Backend.Servicios;
using Microsoft.EntityFrameworkCore;

namespace recTivo.Backend.Repos
{
    public class EscandalloRepository : GenericRepository<Escandallo>, IEscandalloRepository
    {

        public EscandalloRepository(RectivoContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Escandallo>> GetAllAsync()
        {
            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .ToListAsync();
        }

        public override async Task<Escandallo?> GetByIdAsync(params object[] keyValues)
        {
            if (keyValues == null || keyValues.Length == 0)
                return null;

            int id = (int)keyValues[0];

            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .FirstOrDefaultAsync(e => e.IdEscandallo == id);
        }

        public async Task<List<Escandallo>> GetAllEscandallосAsync()
        {
            return await _context.Escandallos
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ComponenteEscandallo>> GetAllComponentesAsync()
        {
            return await _context.ComponenteEscandallos
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Ubicacion>> GetUbicacionesByArticuloAsync(int idArticulo)
        {
            return await _context.Ubicacion
                .Where(u => u.IdArticulo == idArticulo && u.Cantidad > 0)
                .OrderByDescending(u => u.Cantidad)
                .ToListAsync();
        }

        public async Task InsertComponenteAsync(ComponenteEscandallo comp)
        {
            await _context.ComponenteEscandallos.AddAsync(comp);
            await _context.SaveChangesAsync();
        }

        public async Task<Escandallo?> GetByCodigoProductoAsync(string codigoProducto)
        {
            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .FirstOrDefaultAsync(e => e.CodigoProducto == codigoProducto);
        }

        public async Task<List<ComponenteEscandallo>> GetComponentesByEscandalloAsync(int idEscandallo)
        {
            return await _context.ComponenteEscandallos
                                 .Where(c => c.IdEscandallo == idEscandallo)
                                 .OrderBy(c => c.CodigoArticulo)
                                 .ToListAsync();
        }

        public override async Task AddAsync(Escandallo esc)
        {
            await _context.Escandallos.AddAsync(esc);
            await _context.SaveChangesAsync();
            await _context.Entry(esc).ReloadAsync();
        }

        public override void Remove(Escandallo entidad)
        {
            if (entidad != null)
            {
                _context.Escandallos.Remove(entidad);
                _context.SaveChanges();
            }
        }

        public async Task DeleteComponenteAsync(int idComponente)
        {
            var componente = await _context.ComponenteEscandallos
                .FirstOrDefaultAsync(c => c.IdComponente == idComponente);

            if (componente != null)
            {
                _context.ComponenteEscandallos.Remove(componente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            var entidad = await _context.Escandallos.FindAsync(id);
            if (entidad != null)
            {
                Remove(entidad);
            }
        }
    }
}