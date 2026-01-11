using recTivo.Backend.Modelos;
using recTivo.Backend.Servicios;        // Necesario para IEscandalloRepository
using Microsoft.EntityFrameworkCore;

namespace recTivo.Backend.Repos
{
    public class EscandalloRepository : GenericRepository<Escandallo>, IEscandalloRepository
    {
        private readonly RectivoContext _context;

        public EscandalloRepository(RectivoContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los escandallos con sus componentes
        /// </summary>
        public override async Task<IEnumerable<Escandallo>> GetAllAsync()
        {
            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .ToListAsync();
        }

        /// <summary>
        /// Obtiene un escandallo por ID
        /// </summary>
        public override async Task<Escandallo?> GetByIdAsync(params object[] keyValues)
        {
            if (keyValues == null || keyValues.Length == 0)
                return null;

            int id = (int)keyValues[0];

            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .FirstOrDefaultAsync(e => e.IdEscandallo == id);
        }

        public async Task InsertComponenteAsync(ComponenteEscandallo comp)
        {
            await _context.ComponenteEscandallos.AddAsync(comp);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Método personalizado: obtener escandallo por CódigoProducto
        /// </summary>
        public async Task<Escandallo?> GetByCodigoProductoAsync(string codigoProducto)
        {
            return await _context.Escandallos
                                 .Include(e => e.Componentes)
                                 .FirstOrDefaultAsync(e => e.CodigoProducto == codigoProducto);
        }

        /// <summary>
        /// Obtiene todos los componentes de un escandallo por su IdEscandallo
        /// </summary>
        public async Task<List<ComponenteEscandallo>> GetComponentesByEscandalloAsync(int idEscandallo)
        {
            return await _context.ComponenteEscandallos
                                 .Where(c => c.IdEscandallo == idEscandallo)
                                 .OrderBy(c => c.CodigoArticulo)
                                 .ToListAsync();
        }


        /// <summary>
        /// Elimina un escandallo por entidad
        /// </summary>
        public override void Remove(Escandallo entidad)
        {
            if (entidad != null)
            {
                _context.Escandallos.Remove(entidad);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Elimina un escandallo por su ID
        /// </summary>
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
