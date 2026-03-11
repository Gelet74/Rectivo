using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Repos
{
    public class PedidoRepository
    {
        private readonly RectivoContext _context;

        public PedidoRepository(RectivoContext context)
        {
            _context = context;
        }

        // ── Obtener todos con cliente y líneas ──────────────────────────
        public async Task<List<Pedido>> GetAllAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Lineas)
                    .ThenInclude(l => l.Articulo)
                .OrderByDescending(p => p.IdPedido)
                .ToListAsync();
        }

        // ── Obtener por ID ──────────────────────────────────────────────
        public async Task<Pedido?> GetByIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Lineas)
                    .ThenInclude(l => l.Articulo)
                .FirstOrDefaultAsync(p => p.IdPedido == id);
        }

        // ── Crear pedido con sus líneas ─────────────────────────────────
        public async Task AddAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task CerrarPedidoAsync(
            int idPedido,
            Dictionary<string, int> ubicacionesPorArticulo)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = await GetByIdAsync(idPedido)
                    ?? throw new Exception($"Pedido {idPedido} no encontrado.");

                if (pedido.Estado == "Entregado")
                    throw new Exception("El pedido ya está entregado.");

                foreach (var linea in pedido.Lineas)
                {
                    var articulo = await _context.Articulos
                        .FirstOrDefaultAsync(a => a.Codigo == linea.CodigoArticulo);

                    if (articulo != null)
                        articulo.Stock = (byte)Math.Max(0, (articulo.Stock ?? 0) - linea.Cantidad);

                    if (ubicacionesPorArticulo.TryGetValue(linea.CodigoArticulo, out int idUbicacion))
                    {
                        var ub = await _context.Ubicacion
                            .FirstOrDefaultAsync(u => u.IdUbicacion == idUbicacion);

                        if (ub != null)
                            ub.Cantidad = Math.Max(0, ub.Cantidad - linea.Cantidad);
                    }
                }

                pedido.Estado = "Entregado";
                pedido.FechaEntrega = DateTime.Today;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int idPedido)
        {
            var pedido = await _context.Pedidos.FindAsync(idPedido)
                ?? throw new Exception($"Pedido {idPedido} no encontrado.");

            if (pedido.Estado == "Entregado")
                throw new Exception("No se puede eliminar un pedido ya entregado.");

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
        }
    }
}