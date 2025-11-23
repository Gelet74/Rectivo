using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(DbContext context) : base(context) { }

    public async Task<Cliente?> GetByDniAsync(string dni)
        => await _dbSet.FirstOrDefaultAsync(c => c.Dni == dni);
}