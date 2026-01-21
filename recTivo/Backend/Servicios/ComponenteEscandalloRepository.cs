using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos
{
    public class ComponenteEscandalloRepository : GenericRepository<ComponenteEscandallo>, IRepository<ComponenteEscandallo>
    {
        private readonly RectivoContext _context;

        public ComponenteEscandalloRepository(RectivoContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los componentes del escandallo (incluye escandallo)
        /// </summary>
        public override async Task<IEnumerable<ComponenteEscandallo>> GetAllAsync()
        {
            return await _context.ComponenteEscandallos
                                 .Include(c => c.Escandallo)
                                 .ToListAsync();
        }

        /// <summary>
        /// Obtiene un componente específico por ID
        /// </summary>
        public override async Task<ComponenteEscandallo?> GetByIdAsync(params object[] keyValues)
        {
            if (keyValues.Length != 1 || !(keyValues[0] is int id))
                throw new ArgumentException("Se requiere un único parámetro de tipo int para el ID del componente.");

            return await _context.ComponenteEscandallos
                                 .Include(c => c.Escandallo)
                                 .FirstOrDefaultAsync(c => c.IdComponente == id);
        }

        /// <summary>
        /// Obtiene todos los componentes de un escandallo
        /// </summary>
        public async Task<IEnumerable<ComponenteEscandallo>> GetByEscandallo(int idEscandallo)
        {
            return await _context.ComponenteEscandallos
                                 .Where(c => c.IdEscandallo == idEscandallo)
                                 .Include(c => c.Escandallo)
                                 .ToListAsync();
        }

        public async Task InsertComponenteAsync(ComponenteEscandallo componente)
        {
            // Asegurar que EF no intenta mapear propiedades no mapeadas
            componente.Hijos = null;

            _context.ComponenteEscandall​os.Add(componente);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Elimina un componente por su ID
        /// </summary>
        public override void Remove(ComponenteEscandallo componente)
        {
            if (componente != null)
            {
                _context.ComponenteEscandallos.Remove(componente);
                _context.SaveChanges();
            }
        }
    }
}

