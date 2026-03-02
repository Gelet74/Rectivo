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

    /// <summary>Cierra una fase: guarda OK/defectos, descuenta MP y activa la siguiente.
    /// Si es la última fase cierra la orden y sube el PS a stock con ubicación.</summary>
    public async Task CerrarFaseAsync(
        int idOrdenFase,
        int cantidadOK,
        int cantidadDefecto,
        int idEmpleado,
        DateTime fechaCierre,
        RectivoContext context)
    {
        var fase = await _dbSet
            .Include(f => f.OrdenNavigation)
            .FirstOrDefaultAsync(f => f.IdOrdenFase == idOrdenFase)
            ?? throw new Exception($"Fase {idOrdenFase} no encontrada.");

        if (fase.EstadoEnum == EstadoOrden.Cerrada)
            throw new Exception("Esta fase ya está cerrada.");

        // Validar que la fase anterior esté cerrada
        if (fase.NumeroFase > 1)
        {
            var faseAnterior = await _dbSet
                .FirstOrDefaultAsync(f => f.IdOrden == fase.IdOrden &&
                                          f.NumeroFase == fase.NumeroFase - 1);
            if (faseAnterior != null && faseAnterior.EstadoEnum != EstadoOrden.Cerrada)
                throw new Exception("No se puede cerrar esta fase: la fase anterior aún no está cerrada.");
        }

        if (cantidadOK + cantidadDefecto > fase.CantidadEntrada)
            throw new Exception(
                $"OK ({cantidadOK}) + Defectos ({cantidadDefecto}) " +
                $"superan la entrada ({fase.CantidadEntrada}).");

        // ── 1) Cerrar la fase ─────────────────────────────────────────────
        fase.CantidadOK = cantidadOK;
        fase.CantidadDefecto = cantidadDefecto;
        fase.IdEmpleado = idEmpleado;
        fase.FechaFin = fechaCierre;
        fase.EstadoEnum = EstadoOrden.Cerrada;
        await SaveChangesAsync();

        // ── 2) Descontar MP asociadas al componente de fase ───────────────
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
                        f.NumeroFase == fase.NumeroFase + 1)
            .FirstOrDefaultAsync();

        if (siguienteFase != null)
        {
            siguienteFase.CantidadEntrada = cantidadOK;
            await SaveChangesAsync();
        }
        else
        {
            // ── 4) Última fase cerrada → cerrar la orden y subir PS a stock con ubicación ──
            var orden = fase.OrdenNavigation;

            // Cerrar la orden
            orden.Estado = nameof(EstadoOrden.Cerrada);
            await context.SaveChangesAsync();

            // Subir el PS a stock con ubicación automática (pasillo P, estantería 1)
            var articuloPS = await context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPS != null && cantidadOK > 0)
            {
                // Buscar ubicación existente del artículo o crear una nueva
                var ubicacion = await context.Ubicacion
                    .FirstOrDefaultAsync(u => u.IdArticulo == articuloPS.IdArticulo);

                if (ubicacion == null)
                {
                    // Crear ubicación en pasillo P (Producto semiterminado)
                    int maxHueco = await context.Ubicacion
                        .Where(u => u.LetraPasillo == "P" && u.NumeroEstanteria == 1)
                        .MaxAsync(u => (int?)u.Numero) ?? 0;

                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = "P",
                        NumeroEstanteria = 1,
                        Numero = maxHueco + 1,
                        IdArticulo = articuloPS.IdArticulo,
                        Cantidad = cantidadOK
                    };
                    context.Ubicacion.Add(ubicacion);
                }
                else
                {
                    ubicacion.Cantidad += cantidadOK;
                    context.Ubicacion.Update(ubicacion);
                }

                // Recalcular stock total del PS
                await context.SaveChangesAsync();
                articuloPS.Stock = await context.Ubicacion
                    .Where(u => u.IdArticulo == articuloPS.IdArticulo)
                    .SumAsync(u => u.Cantidad);

                await context.SaveChangesAsync();
            }
        }
    }
}
