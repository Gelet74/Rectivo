using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class OrdenRepository : GenericRepository<Orden>, IOrdenRepository
{
    public OrdenRepository(RectivoContext context) : base(context) { }

    public async Task<Orden?> GetByCodigoAsync(string codigo)
        => await _dbSet.FirstOrDefaultAsync(o => o.Codigo == codigo);

    /// <summary>
    /// Busca orden existente con mismo código y misma fecha fin (para agrupar al generar).
    /// Solo agrupa órdenes que estén en estado Pendiente.
    /// </summary>
    public async Task<Orden?> GetByCodigoYFechaAsync(string codigo, DateTime? fechaFin)
    {
        string estadoPendiente = nameof(EstadoOrden.Pendiente);

        if (fechaFin == null)
            return await _dbSet.FirstOrDefaultAsync(o =>
                o.Codigo == codigo &&
                o.FechaFin == null &&
                o.Estado == estadoPendiente);

        return await _dbSet.FirstOrDefaultAsync(o =>
            o.Codigo == codigo &&
            o.FechaFin.HasValue &&
            o.FechaFin.Value.Date == fechaFin.Value.Date &&
            o.Estado == estadoPendiente);
    }

    /// <summary>
    /// Todas las órdenes pendientes (para listar / dashboard).
    /// </summary>
    public async Task<List<Orden>> GetPendientesAsync()
        => await _dbSet
            .Where(o => o.Estado == nameof(EstadoOrden.Pendiente))
            .Include(o => o.IdEmpleadoNavigation)
            .OrderBy(o => o.FechaFin)
            .ToListAsync();

    /// <summary>
    /// Cambia el estado de una orden y guarda.
    /// </summary>
    public async Task CambiarEstadoAsync(int idOrden, EstadoOrden nuevoEstado)
    {
        var orden = await _dbSet.FindAsync(idOrden);
        if (orden != null)
        {
            orden.EstadoEnum = nuevoEstado;
            await SaveChangesAsync();
        }
    }
}
