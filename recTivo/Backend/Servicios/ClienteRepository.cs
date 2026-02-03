using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(RectivoContext context) : base(context) { }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<Cliente?> GetByDniAsync(string dni)
        => await _dbSet.FirstOrDefaultAsync(c => c.Dni == dni);

    public async Task<Cliente?> GetByUsuarioAsync(string usuario)
        => await _dbSet.FirstOrDefaultAsync(c => c.Usuario == usuario);

    public async Task<bool> UsuarioExisteAsync(string usuario)
        => await _dbSet.AnyAsync(c => c.Usuario == usuario);

    public async Task<Cliente?> LoginAsync(string usuario, string password)
        => await _dbSet.FirstOrDefaultAsync(c =>
            c.Usuario == usuario && c.Password == password);


}
