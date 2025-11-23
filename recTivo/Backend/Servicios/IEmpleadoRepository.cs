using System.Threading.Tasks;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<Empleado?> GetByUsernameAsync(string username);
}