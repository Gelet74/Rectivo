using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos;

public class OrdenFaseRepository : GenericRepository<OrdenFase>
{
    public OrdenFaseRepository(RectivoContext context) : base(context) { }

    public async Task<List<OrdenFase>> GetByOrdenAsync(int idOrden)
        => await _dbSet
            .Where(f => f.IdOrden == idOrden)
            .Include(f => f.EmpleadoNavigation)
            .OrderBy(f => f.NumeroFase)
            .ToListAsync();

    public async Task<OrdenFase?> GetSiguientePendienteAsync(int idOrden)
        => await _dbSet
            .Where(f => f.IdOrden == idOrden &&
                        f.Estado == nameof(EstadoOrden.Pendiente))
            .OrderBy(f => f.NumeroFase)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Cierra una fase. Si es la última, cierra la orden y sube el PS
    /// a la ubicación indicada por el usuario (pasillo, estantería, hueco).
    /// </summary>
    public async Task CerrarFaseAsync(
        int idOrdenFase,
        int cantidadOK,
        int cantidadDefecto,
        int idEmpleado,
        DateTime fechaCierre,
        RectivoContext context,
        string? ubicacionPasillo = null,
        int? ubicacionEstanteria = null,
        int? ubicacionHueco = null)
    {
        var fase = await _dbSet
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
        await _context.SaveChangesAsync();

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
            await _context.SaveChangesAsync();
        }
        else
        {
            // ── 4) Última fase → cerrar orden y subir PS a stock ──────────

            var orden = await context.Orden
                .FirstOrDefaultAsync(o => o.IdOrden == fase.IdOrden)
                ?? throw new Exception("Orden no encontrada.");

            orden.Estado = nameof(EstadoOrden.Cerrada);
            await context.SaveChangesAsync();

            var articuloPS = await context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPS != null && cantidadOK > 0)
            {
                // Usar ubicación introducida por el usuario
                string pasillo = ubicacionPasillo ?? "P";
                int estanteria = ubicacionEstanteria ?? 1;
                int hueco = ubicacionHueco ?? 1;

                var ubicacion = await context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.IdArticulo == articuloPS.IdArticulo &&
                        u.LetraPasillo == pasillo &&
                        u.NumeroEstanteria == estanteria &&
                        u.Numero == hueco);

                if (ubicacion == null)
                {
                    // Crear la ubicación indicada por el usuario
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = pasillo,
                        NumeroEstanteria = estanteria,
                        Numero = hueco,
                        IdArticulo = articuloPS.IdArticulo,
                        Cantidad = cantidadOK
                    };
                    context.Ubicacion.Add(ubicacion);
                }
                else
                {
                    // Ya existe esa ubicación para ese artículo → sumar
                    ubicacion.Cantidad += cantidadOK;
                }

                await context.SaveChangesAsync();

                // Recalcular stock total sumando todas las ubicaciones del artículo
                articuloPS.Stock = await context.Ubicacion
                    .Where(u => u.IdArticulo == articuloPS.IdArticulo)
                    .SumAsync(u => u.Cantidad);

                await context.SaveChangesAsync();
            }
        }
    }
}