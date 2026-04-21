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
    /// Cierra una fase.
    /// - Fases PS (01/02/03): descuenta MP y sube PS a stock en la última fase.
    /// - Fase PT (AGRUPAMIENTO): descuenta PS del stock y sube PT a stock con ubicación.
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

        var orden = await _context.Orden
            .FirstOrDefaultAsync(o => o.IdOrden == fase.IdOrden)
            ?? throw new Exception("Orden no encontrada.");

        bool esFasePT = fase.CodigoFase == "AGRUPAMIENTO";

        bool esUltimaFase = !await _context.Set<OrdenFase>()
            .AnyAsync(f => f.IdOrden == fase.IdOrden &&
                           f.NumeroFase == fase.NumeroFase + 1);

        if (esUltimaFase && ubicacionPasillo != null)
        {
            string pasilloVal = ubicacionPasillo;
            int estanteriaVal = ubicacionEstanteria ?? 1;
            int huecoVal = ubicacionHueco ?? 1;

            var articuloPrevio = await _context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPrevio != null)
            {
                var ubicacionOcupada = await _context.Ubicacion
                    .Include(u => u.Articulo)
                    .FirstOrDefaultAsync(u =>
                        u.LetraPasillo == pasilloVal &&
                        u.NumeroEstanteria == estanteriaVal &&
                        u.Numero == huecoVal &&
                        u.IdArticulo != null &&
                        u.IdArticulo != articuloPrevio.IdArticulo);

                if (ubicacionOcupada != null)
                    throw new Exception(
                        $"La ubicación {pasilloVal}-{estanteriaVal}-{huecoVal} ya está ocupada " +
                        $"por el artículo '{ubicacionOcupada.Articulo?.Codigo ?? ubicacionOcupada.IdArticulo.ToString()}'.");
            }
        }

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

        fase.CantidadOK = cantidadOK;
        fase.CantidadDefecto = cantidadDefecto;
        fase.IdEmpleado = idEmpleado;
        fase.FechaFin = fechaCierre;
        fase.EstadoEnum = EstadoOrden.Cerrada;
        await _context.SaveChangesAsync();

        if (esFasePT)
        {
            await CerrarFasePTAsync(orden, cantidadOK, ubicacionPasillo, ubicacionEstanteria, ubicacionHueco);
        }
        else
        {
            await CerrarFasePSAsync(fase, orden, cantidadOK, ubicacionPasillo, ubicacionEstanteria, ubicacionHueco, esUltimaFase);
        }
    }

    // ================================================================
    //   LÓGICA CIERRE FASE PT (agrupamiento)
    // ================================================================
    private async Task CerrarFasePTAsync(
        Orden orden,
        int cantidadOK,
        string? ubicacionPasillo,
        int? ubicacionEstanteria,
        int? ubicacionHueco)
    {
   
        var escandallo = await _context.Escandallos
            .FirstOrDefaultAsync(e => e.CodigoProducto == orden.Codigo);

        if (escandallo != null)
        {
            var componentesPS = await _context.ComponenteEscandallos
                .Where(c => c.IdEscandallo == escandallo.IdEscandallo &&
                             c.CodigoArticulo.StartsWith("PS"))
                .ToListAsync();

            foreach (var comp in componentesPS)
            {
                int cantidadPS = (int)Math.Ceiling((comp.Cantidad ?? 1) * cantidadOK);

                var articuloPS = await _context.Articulos
                    .FirstOrDefaultAsync(a => a.Codigo == comp.CodigoArticulo);

                if (articuloPS != null)
                {
                    // Descontar de ubicaciones (FIFO: las más antiguas primero)
                    var ubicaciones = await _context.Ubicacion
                        .Where(u => u.IdArticulo == articuloPS.IdArticulo && u.Cantidad > 0)
                        .OrderBy(u => u.IdUbicacion)
                        .ToListAsync();

                    decimal pendienteDescontar = cantidadPS;
                    foreach (var ubi in ubicaciones)
                    {
                        if (pendienteDescontar <= 0) break;
                        decimal descontar = Math.Min(ubi.Cantidad ?? 0, pendienteDescontar);
                        ubi.Cantidad = (ubi.Cantidad ?? 0) - descontar;
                        pendienteDescontar -= descontar;
                    }

                    // Actualizar stock total del PS
                    articuloPS.Stock = await _context.Ubicacion
                        .Where(u => u.IdArticulo == articuloPS.IdArticulo)
                        .SumAsync(u => u.Cantidad);

                    if (articuloPS.Stock < 0) articuloPS.Stock = 0;
                }
            }
            await _context.SaveChangesAsync();
        }

        // 3) Cerrar la orden PT
        orden.Estado = nameof(EstadoOrden.Cerrada);
        await _context.SaveChangesAsync();

        // 4) Subir el PT a stock con ubicación
        if (cantidadOK > 0 && ubicacionPasillo != null)
        {
            var articuloPT = await _context.Articulos
                .FirstOrDefaultAsync(a => a.Codigo == orden.Codigo);

            if (articuloPT != null)
            {
                string pasillo = ubicacionPasillo;
                int estanteria = ubicacionEstanteria ?? 1;
                int hueco = ubicacionHueco ?? 1;

                var ubicacion = await _context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.IdArticulo == articuloPT.IdArticulo &&
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
                        IdArticulo = articuloPT.IdArticulo,
                        Cantidad = cantidadOK
                    });
                }
                else
                {
                    ubicacion.Cantidad += cantidadOK;
                }

                await _context.SaveChangesAsync();

                articuloPT.Stock = await _context.Ubicacion
                    .Where(u => u.IdArticulo == articuloPT.IdArticulo)
                    .SumAsync(u => u.Cantidad);

                await _context.SaveChangesAsync();
            }
        }
    }

    // ================================================================
    //   LÓGICA CIERRE FASE PS (original)
    // ================================================================
    private async Task CerrarFasePSAsync(
        OrdenFase fase,
        Orden orden,
        int cantidadOK,
        string? ubicacionPasillo,
        int? ubicacionEstanteria,
        int? ubicacionHueco,
        bool esUltimaFase)
    {
        // Descontar MP asociadas al componente de fase
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

        // Propagar CantidadOK a la siguiente fase (si existe)
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
            // Última fase PS → cerrar orden y subir PS a stock
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