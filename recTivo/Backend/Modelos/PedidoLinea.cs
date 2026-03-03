namespace recTivo.Backend.Modelos
{
    public class PedidoLinea
    {
        public int IdPedidoLinea { get; set; }
        public int IdPedido { get; set; }
        public string CodigoArticulo { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }   // PVP calculado en el momento
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Navegación
        public Pedido? Pedido { get; set; }
        public Articulo? Articulo { get; set; }
    }
}