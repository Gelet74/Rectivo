using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class VistaArticulosUbicacionRepository : GenericRepository<VistaArticuloUbicacion>, IVistaArticulosUbicacionRepository
{
    public VistaArticulosUbicacionRepository(DbContext context) : base(context) { }

    // Si la vista es solo lectura, se puede evitar SaveChanges en la capa de uso.
}