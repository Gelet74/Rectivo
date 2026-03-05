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
    string? ubicacionPasillo = null,
    int? ubicacionEstanteria = null,
    int? ubicacionHueco = null)
    {
        var fase = await _context.Set<OrdenFase>()
            .FirstOrDefaultAsync(f => f.IdOrdenFase == idOrdenFase)
            ?? throw new Exception($"Fase {idOrdenFase} no encontrada.");

        if (fase.EstadoEnum == EstadoOrden.Cerrada)
            throw new Exception("Esta fase ya está cerrada.");

        // ── VALIDACIÓN PREVIA: ubicación antes de tocar nada ─────────────
        bool esUltimaFase = !await _context.Set<OrdenFase>()
            .AnyAsync(f => f.IdOrden == fase.IdOrden &&
                           f.NumeroFase == fase.NumeroFase + 1);

        if (esUltimaFase && ubicacionPasillo != null)
        {
            string pasilloVal = ubicacionPasillo;
            int estanteriaVal = ubicacionEstanteria ?? 1;
            int huecoVal = ubicacionHueco ?? 1;

            var ordenPrevia = await _context.Orden
                .FirstOrDefaultAsync(o => o.IdOrden == fase.IdOrden)
                ?? throw new Exception("Orden no encontrada.");

            var articuloPSPrevio = await _context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == ordenPrevia.Codigo);

            if (articuloPSPrevio != null)
            {
                var ubicacionOcupadaPrevia = await _context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.LetraPasillo == pasilloVal &&
                        u.NumeroEstanteria == estanteriaVal &&
                        u.Numero == huecoVal &&
                        u.IdArticulo != null &&
                        u.IdArticulo != articuloPSPrevio.IdArticulo);

                if (ubicacionOcupadaPrevia != null)
                    throw new Exception(
                        $"La ubicación {pasilloVal}-{estanteriaVal}-{huecoVal} ya está ocupada " +
                        $"por el artículo '{ubicacionOcupadaPrevia.Articulo?.Codigo ?? ubicacionOcupadaPrevia.IdArticulo.ToString()}'.");
            }
        }
        // ─────────────────────────────────────────────────────────────────

        // Validar que la fase anterior esté cerrada
        if (fase.NumeroFase > 1)
        {
            var faseAnterior = await _context.Set<OrdenFase>()
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
        var escandallo = await _context.Escandallos
            .FirstOrDefaultAsync(e => e.CodigoProducto == fase.CodigoFase);

        if (escandallo != null)
        {
            var componentes = await _context.ComponenteEscandallos
                .Where(c => c.IdEscandallo == escandallo.IdEscandallo &&
                             c.CodigoArticulo.StartsWith("MP"))
                .ToListAsync();

            foreach (var comp in componentes)
            {
                decimal cantidadMP = (comp.Cantidad ?? 1) * fase.CantidadEntrada;
                var articulo = await _context.Articulos
                    .FirstOrDefaultAsync(a => a.Codigo == comp.CodigoArticulo);

                if (articulo != null)
                {
                    articulo.Stock -= (int)Math.Ceiling(cantidadMP);
                    if (articulo.Stock < 0) articulo.Stock = 0;
                }
            }
            await _context.SaveChangesAsync();
        }

        // ── 3) Propagar CantidadOK a la siguiente fase (si existe) ────────
        var siguienteFase = await _context.Set<OrdenFase>()
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
            var orden = await _context.Orden
                .FirstOrDefaultAsync(o => o.IdOrden == fase.IdOrden)
                ?? throw new Exception("Orden no encontrada.");

            orden.Estado = nameof(EstadoOrden.Cerrada);
            await _context.SaveChangesAsync();

            var articuloPS = await _context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPS != null && cantidadOK > 0)
            {
                string pasillo = ubicacionPasillo ?? "P";
                int estanteria = ubicacionEstanteria ?? 1;
                int hueco = ubicacionHueco ?? 1;

                var ubicacion = await _context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.IdArticulo == articuloPS.IdArticulo &&
                        u.LetraPasillo == pasillo &&
                        u.NumeroEstanteria == estanteria &&
                        u.Numero == hueco);

                if (ubicacion == null)
                {
                    _context.Ubicacion.Add(new Ubicacion
                    {
                        LetraPasillo = pasillo,
                        NumeroEstanteria = estanteria,
                        Numero = hueco,
                        IdArticulo = articuloPS.IdArticulo,
                        Cantidad = cantidadOK
                    });
                }
                else
                {
                    ubicacion.Cantidad += cantidadOK;
                }

                await _context.SaveChangesAsync();

                articuloPS.Stock = await _context.Ubicacion
                    .Where(u => u.IdArticulo == articuloPS.IdArticulo)
                    .SumAsync(u => u.Cantidad);

                await _context.SaveChangesAsync();
            }
        }
    }
}