namespace recTivo.Backend.Modelos
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.Today;
        public DateTime? FechaEntrega { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente | Entregado
        public decimal Total { get; set; }

        // Navegación
        public Cliente? Cliente { get; set; }
        public List<PedidoLinea> Lineas { get; set; } = new();

        // Ignorado en BD
        public string EstadoTexto => Estado switch
        {
            "Pendiente" => "Pendiente",
            "Entregado" => "Entregado",
            _ => Estado
        };
    }
}