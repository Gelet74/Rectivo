using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class OrdenFaseRepository : GenericRepository<OrdenFase>
{
    public OrdenFaseRepository(RectivoContext context) : base(context) { }

    /// <summary>Fases de una orden, ordenadas por NumeroFase.</summary>
    public async Task<List<OrdenFase>> GetByOrdenAsync(int idOrden)
        => await _dbSet
            .Where(f => f.IdOrden == idOrden)
            .Include(f => f.EmpleadoNavigation)
            .OrderBy(f => f.NumeroFase)
            .ToListAsync();

    /// <summary>Primera fase Pendiente de una orden (la siguiente a ejecutar).</summary>
    public async Task<OrdenFase?> GetSiguientePendienteAsync(int idOrden)
        => await _dbSet
            .Where(f => f.IdOrden == idOrden &&
                        f.Estado == nameof(EstadoOrden.Pendiente))
            .OrderBy(f => f.NumeroFase)
            .FirstOrDefaultAsync();

    /// <summary>Cierra una fase: guarda OK/defectos, descuenta MP y activa la siguiente.</summary>
    public async Task CerrarFaseAsync(
        int idOrdenFase,
        int cantidadOK,
        int cantidadDefecto,
        int idEmpleado,
        RectivoContext context)
    {
        var fase = await _dbSet
            .Include(f => f.OrdenNavigation)
            .FirstOrDefaultAsync(f => f.IdOrdenFase == idOrdenFase)
            ?? throw new Exception($"Fase {idOrdenFase} no encontrada.");

        if (fase.EstadoEnum == EstadoOrden.Cerrada)
            throw new Exception("Esta fase ya está cerrada.");

        if (cantidadOK + cantidadDefecto > fase.CantidadEntrada)
            throw new Exception(
                $"OK ({cantidadOK}) + Defectos ({cantidadDefecto}) " +
                $"superan la entrada ({fase.CantidadEntrada}).");

        // ── 1) Cerrar la fase ─────────────────────────────────────────────
        fase.CantidadOK = cantidadOK;
        fase.CantidadDefecto = cantidadDefecto;
        fase.IdEmpleado = idEmpleado;
        fase.EstadoEnum = EstadoOrden.Cerrada;
        await SaveChangesAsync();

        // ── 2) Descontar MP asociadas al componente de fase ───────────────
        //    Buscamos el escandallo del artículo de fase (01x, 02x, 03x)
        //    y de sus componentes MP descontamos stock proporcional a CantidadEntrada
        var escandallo = await context.Escandallos
            .FirstOrDefaultAsync(e => e.CodigoProducto == fase.CodigoFase);

        if (escandallo != null)
        {
            var componentes = await context.ComponenteEscandallos
                .Where(c => c.IdEscandallo == escandallo.IdEscandallo &&
                             c.CodigoArticulo.StartsWith("MP"))
                .ToListAsync();

            foreach (var comp in componentes)
            {
                decimal cantidadMP = (comp.Cantidad ?? 1) * fase.CantidadEntrada;
                var articulo = await context.Articulos
                    .FirstOrDefaultAsync(a => a.Codigo == comp.CodigoArticulo);

                if (articulo != null)
                {
                    articulo.Stock -= (int)Math.Ceiling(cantidadMP);
                    if (articulo.Stock < 0) articulo.Stock = 0;
                }
            }

            await context.SaveChangesAsync();
        }

        // ── 3) Propagar CantidadOK a la siguiente fase (si existe) ────────
        var siguienteFase = await _dbSet
            .Where(f => f.IdOrden == fase.IdOrden &&
                        f.NumeroFase == fase.NumeroFase + 1 &&
                        f.Estado == nameof(EstadoOrden.Pendiente))
            .FirstOrDefaultAsync();

        if (siguienteFase != null)
        {
            siguienteFase.CantidadEntrada = cantidadOK;
            await SaveChangesAsync();
        }
        else
        {
            // ── 4) Última fase cerrada → el PS va a stock ─────────────────
            var orden = fase.OrdenNavigation;
            var articuloPS = await context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPS != null)
            {
                articuloPS.Stock += cantidadOK;
                await context.SaveChangesAsync();
            }
        }
    }
}
