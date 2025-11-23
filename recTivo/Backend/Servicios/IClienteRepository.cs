using System.Threading.Tasks;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByDniAsync(string dni);
}