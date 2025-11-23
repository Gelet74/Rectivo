using System.Threading.Tasks;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public interface IOrdenRepository : IRepository<Orden>
{
    Task<Orden?> GetByCodigoAsync(string codigo);
}