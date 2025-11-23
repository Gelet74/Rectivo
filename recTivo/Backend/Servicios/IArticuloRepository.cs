using System.Threading.Tasks;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public interface IArticuloRepository : IRepository<Articulo>
{
    Task<Articulo?> GetByCodigoAsync(string codigo);
}