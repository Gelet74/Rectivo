using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class OrdenRepository : GenericRepository<Orden>, IOrdenRepository
{
    public OrdenRepository(DbContext context) : base(context) { }

    public async Task<Orden?> GetByCodigoAsync(string codigo)
        => await _dbSet.FirstOrDefaultAsync(o => o.Codigo == codigo);
}