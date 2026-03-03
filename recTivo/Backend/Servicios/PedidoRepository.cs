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

        // ── Cerrar pedido: marcar Entregado + descontar stock ───────────
        public async Task CerrarPedidoAsync(int idPedido)
        {
            var pedido = await GetByIdAsync(idPedido)
                ?? throw new Exception($"Pedido {idPedido} no encontrado.");

            if (pedido.Estado == "Entregado")
                throw new Exception("El pedido ya está entregado.");

            foreach (var linea in pedido.Lineas)
            {
                // 1) Descontar de articulo.stock
                var articulo = await _context.Articulos
                    .FirstOrDefaultAsync(a => a.Codigo == linea.CodigoArticulo);

                if (articulo != null)
                {
                    articulo.Stock = (byte)Math.Max(0, (int)articulo.Stock - linea.Cantidad);
                }

                // 2) Descontar de ubicacion (FIFO: primero el hueco con más stock)
                var ubicaciones = await _context.Ubicacion
                    .Where(u => u.IdArticulo == articulo!.IdArticulo && u.Cantidad > 0)
                    .OrderByDescending(u => u.Cantidad)
                    .ToListAsync();

                int pendiente = linea.Cantidad;
                foreach (var ub in ubicaciones)
                {
                    if (pendiente <= 0) break;
                    int descontar = Math.Min(ub.Cantidad, pendiente);
                    ub.Cantidad -= descontar;
                    pendiente -= descontar;
                }
            }

            pedido.Estado = "Entregado";
            pedido.FechaEntrega = DateTime.Today;

            await _context.SaveChangesAsync();
        }

        // ── Eliminar pedido (solo si está Pendiente) ────────────────────
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