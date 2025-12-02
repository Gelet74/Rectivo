using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using System.Threading.Tasks;

namespace recTivo.Backend.Servicios
{
    public interface IEscandalloRepository : IRepository<Escandallo>
    {
        /// <summary>
        /// Busca un escandallo por su código de producto
        /// </summary>
        Task<Escandallo?> GetByCodigoProductoAsync(string codigoProducto);
    }
}
