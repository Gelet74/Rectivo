using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recTivo.Backend.Repos
{
    public class RolRepository
    {
        private readonly RectivoContext _context;

        public RolRepository(RectivoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            return await _context.Rols.ToListAsync();
        }

        public async Task<Rol> GetByIdAsync(int id)
        {
            return await _context.Rols.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(Rol rol)
        {
            _context.Rols.Add(rol);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rol rol)
        {
            _context.Rols.Update(rol);
            await _context.SaveChangesAsync();
        }
    }
}
