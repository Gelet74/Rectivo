using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

namespace recTivo.MVVM
{
    // ================================================================
    //  Fila editable en la tabla de líneas del nuevo pedido
    // ================================================================
    public class FilaLineaPedido : MVBase
    {
        public Articulo Articulo { get; set; } = null!;
        public string Codigo => Articulo.Codigo;
        public string Descripcion => Articulo.descrip ?? "";
        public string Descripcion2 => Articulo.descrip2 ?? "";

        private int _cantidad = 1;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                SetProperty(ref _cantidad, value);
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal PrecioUnitario { get; set; }   // PVP calculado
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    // ================================================================
    //  Fila del listado de pedidos existentes
    // ================================================================
    public class FilaPedido
    {
        public Pedido Pedido { get; set; } = null!;
        public int IdPedido => Pedido.IdPedido;
        public string Cliente => Pedido.Cliente != null
            ? $"{Pedido.Cliente.Nombre} {Pedido.Cliente.Apellido1}".Trim()
            : "—";
        public string Fecha => Pedido.FechaPedido.ToString("dd/MM/yyyy");
        public string Entrega => Pedido.FechaEntrega?.ToString("dd/MM/yyyy") ?? "—";
        public string Estado => Pedido.EstadoTexto;
        public decimal Total => Pedido.Total;
    }

    // ================================================================
    //  ViewModel principal
    // ================================================================
    public class MVPedido : MVBase
    {
        private const decimal MARGEN = 1.30m;

        private readonly PedidoRepository _pedidoRepo;
        private readonly ArticuloRepository _articuloRepo;
        private readonly EscandalloRepository _escandalloRepo;
        private readonly ClienteRepository _clienteRepo;

        public MVPedido(
            PedidoRepository pedidoRepo,
            ArticuloRepository articuloRepo,
            EscandalloRepository escandalloRepo,
            ClienteRepository clienteRepo)
        {
            _pedidoRepo = pedidoRepo;
            _articuloRepo = articuloRepo;
            _escandalloRepo = escandalloRepo;
            _clienteRepo = clienteRepo;
        }

        // ── Estado compartido ────────────────────────────────────────
        private List<Articulo> _todosArticulosPT = new();
        private List<Cliente> _todosClientes = new();

        // ================================================================
        //  SECCIÓN: CREAR PEDIDO
        // ================================================================

        private List<Articulo> _articulosPTFiltrados = new();
        public List<Articulo> ArticulosPTFiltrados
        {
            get => _articulosPTFiltrados;
            set => SetProperty(ref _articulosPTFiltrados, value);
        }

        private string _filtroPT = "";
        public string FiltroPT
        {
            get => _filtroPT;
            set { SetProperty(ref _filtroPT, value); AplicarFiltroPT(); }
        }

        public ObservableCollection<FilaLineaPedido> LineasPedido { get; } = new();

        private List<Cliente> _clientesFiltrados = new();
        public List<Cliente> ClientesFiltrados
        {
            get => _clientesFiltrados;
            set => SetProperty(ref _clientesFiltrados, value);
        }

        private string _filtroCliente = "";
        public string FiltroCliente
        {
            get => _filtroCliente;
            set { SetProperty(ref _filtroCliente, value); AplicarFiltroCliente(); }
        }

        private Cliente? _clienteSeleccionado;
        public Cliente? ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set => SetProperty(ref _clienteSeleccionado, value);
        }

        private DateTime? _fechaEntrega;
        public DateTime? FechaEntrega
        {
            get => _fechaEntrega;
            set => SetProperty(ref _fechaEntrega, value);
        }

        public decimal TotalPedido => LineasPedido.Sum(l => l.Subtotal);

        // ================================================================
        //  SECCIÓN: LISTADO DE PEDIDOS
        // ================================================================

        private List<FilaPedido> _todosPedidos = new();

        private ObservableCollection<FilaPedido> _pedidosFiltrados = new();
        public ObservableCollection<FilaPedido> PedidosFiltrados
        {
            get => _pedidosFiltrados;
            set => SetProperty(ref _pedidosFiltrados, value);
        }

        private string? _filtroEstadoPedido;
        public string? FiltroEstadoPedido
        {
            get => _filtroEstadoPedido;
            set { SetProperty(ref _filtroEstadoPedido, value); AplicarFiltrosPedidos(); }
        }

        private string? _filtroCodigoPedido;
        public string? FiltroCodigoPedido
        {
            get => _filtroCodigoPedido;
            set { SetProperty(ref _filtroCodigoPedido, value); AplicarFiltrosPedidos(); }
        }

        public List<string> OpcionesEstado { get; } = new() { "Todos", "Pendiente", "Entregado" };

        // ================================================================
        //  INICIALIZAR
        // ================================================================
        public async Task InicializarAsync()
        {
            _todosArticulosPT = (await _articuloRepo.GetAllAsync())
                .Where(a => a.Codigo.StartsWith("PT"))
                .OrderBy(a => a.Codigo)
                .ToList();

            _todosClientes = (await _clienteRepo.GetAllAsync())
                .OrderBy(c => c.Apellido1)
                .ToList();

            ArticulosPTFiltrados = new List<Articulo>(_todosArticulosPT);
            ClientesFiltrados = new List<Cliente>(_todosClientes);

            await CargarPedidosAsync();
        }

        // ================================================================
        //  FILTROS
        // ================================================================
        private void AplicarFiltroPT()
        {
            if (string.IsNullOrWhiteSpace(FiltroPT))
            { ArticulosPTFiltrados = new List<Articulo>(_todosArticulosPT); return; }

            string f = FiltroPT.Trim().ToLower();
            ArticulosPTFiltrados = _todosArticulosPT
                .Where(a => a.Codigo.ToLower().Contains(f) ||
                            (a.descrip?.ToLower().Contains(f) ?? false) ||
                            (a.descrip2?.ToLower().Contains(f) ?? false))
                .ToList();
        }

        private void AplicarFiltroCliente()
        {
            if (string.IsNullOrWhiteSpace(FiltroCliente))
            { ClientesFiltrados = new List<Cliente>(_todosClientes); return; }

            string f = FiltroCliente.Trim().ToLower();
            ClientesFiltrados = _todosClientes
                .Where(c => (c.Nombre?.ToLower().Contains(f) ?? false) ||
                            (c.Apellido1?.ToLower().Contains(f) ?? false) ||
                            (c.Dni?.ToLower().Contains(f) ?? false))
                .ToList();
        }

        private void AplicarFiltrosPedidos()
        {
            var result = _todosPedidos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroEstadoPedido) && FiltroEstadoPedido != "Todos")
                result = result.Where(p => p.Estado == FiltroEstadoPedido);

            if (!string.IsNullOrWhiteSpace(FiltroCodigoPedido))
                result = result.Where(p =>
                    p.IdPedido.ToString().Contains(FiltroCodigoPedido!) ||
                    p.Cliente.Contains(FiltroCodigoPedido!, StringComparison.OrdinalIgnoreCase));

            PedidosFiltrados = new ObservableCollection<FilaPedido>(result);
        }

        public void FiltrarSoloPendientes()
        {
            FiltroEstadoPedido = "Pendiente";
        }

        // ================================================================
        //  TOGGLE ARTÍCULO PT (checkbox en listbox)
        // ================================================================
        public async Task TogglePT(Articulo articulo, bool marcado)
        {
            if (marcado)
            {
                if (LineasPedido.Any(l => l.Codigo == articulo.Codigo)) return;

                decimal pvp = await CalcularPvpAsync(articulo.Codigo);

                LineasPedido.Add(new FilaLineaPedido
                {
                    Articulo = articulo,
                    Cantidad = 1,
                    PrecioUnitario = pvp
                });
            }
            else
            {
                var fila = LineasPedido.FirstOrDefault(l => l.Codigo == articulo.Codigo);
                if (fila != null) LineasPedido.Remove(fila);
            }

            OnPropertyChanged(nameof(TotalPedido));
        }

        public void NotificarTotalCambiado() => OnPropertyChanged(nameof(TotalPedido));

        // ================================================================
        //  CÁLCULO PVP DESDE ESCANDALLO
        // ================================================================
        private async Task<decimal> CalcularPvpAsync(string codigoPT)
        {
            // FIX 1: Caché por sesión de cálculo — evita recalcular el mismo
            //        subcomponente exponencialmente en escandallos multinivel
            var cache = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            var escandallo = await _escandalloRepo.GetByCodigoProductoAsync(codigoPT);
            if (escandallo == null) return 0;

            // FIX 2: Materializar con ToListAsync antes de recursionar para
            //        cerrar el DataReader antes de abrir queries anidadas
            var componentes = await _escandalloRepo
                .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

            decimal coste = await SumarCosteAsync(componentes, cache);
            return Math.Round(coste * MARGEN, 2);
        }

        // FIX 1+2+3: Caché + lista materializada + algoritmo sin doble conteo
        private async Task<decimal> SumarCosteAsync(
            List<ComponenteEscandallo> componentes,
            Dictionary<string, decimal> cache)
        {
            decimal total = 0;

            foreach (var comp in componentes)
            {
                decimal cantidad = comp.Cantidad ?? 1;

                // Consultar caché primero — evita recalcular el mismo código
                if (cache.TryGetValue(comp.CodigoArticulo, out decimal costeCache))
                {
                    total += cantidad * costeCache;
                    continue;
                }

                // Buscar precio_compra directo en la tabla artículo
                var artGeneral = await _articuloRepo.GetByCodigoAsync(comp.CodigoArticulo);

                if (artGeneral?.PrecioCompra > 0)
                {
                    // Tiene precio directo → sumar y parar, NO recursionar
                    decimal costeUnitario = artGeneral.PrecioCompra ?? 0;
                    cache[comp.CodigoArticulo] = costeUnitario;
                    total += cantidad * costeUnitario;
                    continue;
                }

                // No tiene precio → buscar su sub-escandallo y recursionar
                var subEsc = await _escandalloRepo.GetByCodigoProductoAsync(comp.CodigoArticulo);
                if (subEsc != null)
                {
                    // FIX 2: Materializar ANTES de recursionar para cerrar el DataReader
                    var subComps = await _escandalloRepo
                        .GetComponentesByEscandalloAsync(subEsc.IdEscandallo);

                    decimal costeSubEsc = await SumarCosteAsync(subComps, cache);
                    cache[comp.CodigoArticulo] = costeSubEsc;
                    total += cantidad * costeSubEsc;
                }
            }

            return total;
        }

        // ================================================================
        //  CREAR PEDIDO
        // ================================================================
        public async Task<bool> CrearPedidoAsync()
        {
            if (ClienteSeleccionado == null)
            { MensajeError.Mostrar("VENTAS", "Selecciona un cliente."); return false; }

            if (LineasPedido.Count == 0)
            { MensajeError.Mostrar("VENTAS", "Añade al menos un artículo PT al pedido."); return false; }

            if (LineasPedido.Any(l => l.Cantidad <= 0))
            { MensajeError.Mostrar("VENTAS", "Todas las cantidades deben ser mayores que 0."); return false; }

            try
            {
                var pedido = new Pedido
                {
                    IdCliente = ClienteSeleccionado.IdCliente,
                    FechaPedido = DateTime.Today,
                    FechaEntrega = FechaEntrega,
                    Estado = "Pendiente",
                    Total = TotalPedido,
                    Lineas = LineasPedido.Select(l => new PedidoLinea
                    {
                        CodigoArticulo = l.Codigo,
                        Cantidad = l.Cantidad,
                        PrecioUnitario = l.PrecioUnitario
                    }).ToList()
                };

                await _pedidoRepo.AddAsync(pedido);

                MensajeInformacion.Mostrar("VENTAS",
                    $"Pedido #{pedido.IdPedido} creado correctamente. Total: {TotalPedido:0.00} €", 2);

                LimpiarFormulario();
                await CargarPedidosAsync();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("VENTAS", $"Error al crear el pedido: {ex.Message}");
                return false;
            }
        }

        // ================================================================
        //  CERRAR PEDIDO (descontar stock)
        // ================================================================
        public async Task<bool> CerrarPedidoAsync(FilaPedido fila)
        {
            if (fila.Estado == "Entregado")
            { MensajeError.Mostrar("VENTAS", "Este pedido ya está entregado."); return false; }

            try
            {
                await _pedidoRepo.CerrarPedidoAsync(fila.IdPedido);

                MensajeInformacion.Mostrar("VENTAS",
                    $"Pedido #{fila.IdPedido} cerrado. Stock actualizado.", 2);

                await CargarPedidosAsync();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("VENTAS", $"Error al cerrar el pedido: {ex.Message}");
                return false;
            }
        }

        // ================================================================
        //  CARGAR PEDIDOS
        // ================================================================
        public async Task CargarPedidosAsync()
        {
            try
            {
                var pedidos = await _pedidoRepo.GetAllAsync();
                _todosPedidos = pedidos.Select(p => new FilaPedido { Pedido = p }).ToList();
                AplicarFiltrosPedidos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVPedido] Error: {ex.Message}");
            }
        }

        // ================================================================
        //  LIMPIAR FORMULARIO
        // ================================================================
        private void LimpiarFormulario()
        {
            LineasPedido.Clear();
            ClienteSeleccionado = null;
            FechaEntrega = null;
            FiltroCliente = "";
            FiltroPT = "";
            OnPropertyChanged(nameof(TotalPedido));
        }
    }
}