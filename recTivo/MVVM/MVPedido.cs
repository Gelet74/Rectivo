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
            set { SetProperty(ref _cantidad, value); OnPropertyChanged(nameof(Subtotal)); }
        }

        public decimal PrecioUnitario { get; set; }
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
        private readonly OrdenRepository _ordenRepo;
        private readonly OrdenFaseRepository _ordenFaseRepo;

        public MVPedido(
            PedidoRepository pedidoRepo,
            ArticuloRepository articuloRepo,
            EscandalloRepository escandalloRepo,
            ClienteRepository clienteRepo,
            OrdenRepository ordenRepo,
            OrdenFaseRepository ordenFaseRepo)
        {
            _pedidoRepo = pedidoRepo;
            _articuloRepo = articuloRepo;
            _escandalloRepo = escandalloRepo;
            _clienteRepo = clienteRepo;
            _ordenRepo = ordenRepo;
            _ordenFaseRepo = ordenFaseRepo;
        }

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

        public ObservableCollection<FilaPedido> PedidosFiltrados { get; } = new();

        // Cuando es true los setters de filtro no disparan AplicarFiltrosPedidos,
        // evitando que el ComboBox de Estado borre la selección del DataGrid al inicializar.
        private bool _suprimirFiltros = true;

        private string? _filtroEstadoPedido;
        public string? FiltroEstadoPedido
        {
            get => _filtroEstadoPedido;
            set { SetProperty(ref _filtroEstadoPedido, value); if (!_suprimirFiltros) AplicarFiltrosPedidos(); }
        }

        private string? _filtroCodigoPedido;
        public string? FiltroCodigoPedido
        {
            get => _filtroCodigoPedido;
            set { SetProperty(ref _filtroCodigoPedido, value); if (!_suprimirFiltros) AplicarFiltrosPedidos(); }
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
            _suprimirFiltros = false; // a partir de aquí los filtros funcionan normalmente
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

            var lista = result.ToList();
            PedidosFiltrados.Clear();
            foreach (var p in lista)
                PedidosFiltrados.Add(p);
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
        //  Estrategia: pre-cargar TODO en memoria antes de cualquier recursión
        //  para no tener nunca dos queries abiertas al mismo tiempo en EF Core
        // ================================================================
        private async Task<decimal> CalcularPvpAsync(string codigoPT)
        {
            var todosEscandallos = await _escandalloRepo.GetAllEscandallосAsync();

            var todosComponentes = await _escandalloRepo.GetAllComponentesAsync();

            var todosArticulos = await _articuloRepo.GetAllAsync();

            var escandallosPorCodigo = todosEscandallos
                .ToDictionary(e => e.CodigoProducto, e => e, StringComparer.OrdinalIgnoreCase);

            var componentesPorEscandallo = todosComponentes
                .GroupBy(c => c.IdEscandallo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var articulosPorCodigo = todosArticulos
                .ToDictionary(a => a.Codigo, a => a, StringComparer.OrdinalIgnoreCase);

            var cache = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            if (!escandallosPorCodigo.TryGetValue(codigoPT, out var escandallo))
                return 0;

            if (!componentesPorEscandallo.TryGetValue(escandallo.IdEscandallo, out var componentes))
                return 0;

            decimal coste = SumarCosteEnMemoria(
                componentes,
                escandallosPorCodigo,
                componentesPorEscandallo,
                articulosPorCodigo,
                cache);

            return Math.Round(coste * MARGEN, 2);
        }

        // Recursión 100% en memoria — cero queries a BD
        private decimal SumarCosteEnMemoria(
            List<ComponenteEscandallo> componentes,
            Dictionary<string, Escandallo> escandallosPorCodigo,
            Dictionary<int, List<ComponenteEscandallo>> componentesPorEscandallo,
            Dictionary<string, Articulo> articulosPorCodigo,
            Dictionary<string, decimal> cache)
        {
            decimal total = 0;

            foreach (var comp in componentes)
            {
                decimal cantidad = comp.Cantidad ?? 1;

                // Consultar caché primero
                if (cache.TryGetValue(comp.CodigoArticulo, out decimal costeCache))
                {
                    total += cantidad * costeCache;
                    continue;
                }

                // Tiene precio_compra directo → usarlo, no recursionar
                if (articulosPorCodigo.TryGetValue(comp.CodigoArticulo, out var art)
                    && art.PrecioCompra > 0)
                {
                    decimal costeUnitario = art.PrecioCompra ?? 0;
                    cache[comp.CodigoArticulo] = costeUnitario;
                    total += cantidad * costeUnitario;
                    continue;
                }

                // Sin precio directo → buscar sub-escandallo y recursionar
                if (escandallosPorCodigo.TryGetValue(comp.CodigoArticulo, out var subEsc)
                    && componentesPorEscandallo.TryGetValue(subEsc.IdEscandallo, out var subComps))
                {
                    decimal costeSubEsc = SumarCosteEnMemoria(
                        subComps,
                        escandallosPorCodigo,
                        componentesPorEscandallo,
                        articulosPorCodigo,
                        cache);

                    cache[comp.CodigoArticulo] = costeSubEsc;
                    total += cantidad * costeSubEsc;
                }
            }

            return total;
        }

        // ================================================================
        //  CREAR ÓRDENES DE AGRUPACIÓN — una por artículo PT del pedido
        // ================================================================
        private async Task CrearOrdenesAgrupacionAsync(Pedido pedido)
        {
            // Empleado de sesión
            int idEmpleado = 1; // fallback
            if (System.Windows.Application.Current is App app && app.EmpleadoActual != null)
                idEmpleado = app.EmpleadoActual.Id;

            // Pre-cargar escandallos y artículos en memoria (mismo patrón que CalcularPvpAsync)
            var todosEscandallos = await _escandalloRepo.GetAllEscandallосAsync();
            var todosComponentes = await _escandalloRepo.GetAllComponentesAsync();
            var todosArticulos = await _articuloRepo.GetAllAsync();

            var escandallosPorCodigo = todosEscandallos
                .ToDictionary(e => e.CodigoProducto, e => e, StringComparer.OrdinalIgnoreCase);

            var componentesPorEscandallo = todosComponentes
                .GroupBy(c => c.IdEscandallo)
                .ToDictionary(g => g.Key, g => g.ToList());

            var articulosPorCodigo = todosArticulos
                .ToDictionary(a => a.Codigo, a => a, StringComparer.OrdinalIgnoreCase);

            foreach (var linea in pedido.Lineas)
            {
                // Obtener componentes PS y HE del escandallo del PT
                if (!escandallosPorCodigo.TryGetValue(linea.CodigoArticulo, out var escandallo))
                    continue;

                if (!componentesPorEscandallo.TryGetValue(escandallo.IdEscandallo, out var componentes))
                    continue;

                var componentesPsHe = componentes
                    .Where(c => c.CodigoArticulo.StartsWith("PS", StringComparison.OrdinalIgnoreCase) ||
                                c.CodigoArticulo.StartsWith("HE", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (componentesPsHe.Count == 0)
                    continue;

                if (!articulosPorCodigo.TryGetValue(linea.CodigoArticulo, out var articuloPT))
                    continue;

                var orden = new Orden
                {
                    Codigo = linea.CodigoArticulo,
                    Cantidad = linea.Cantidad,
                    IdArticulo = articuloPT.IdArticulo,
                    IdEmpleado = idEmpleado,
                    FechaFin = pedido.FechaEntrega,
                    Estado = nameof(EstadoOrden.Pendiente)
                };

                await _ordenRepo.AddAsync(orden);

                // Fase única de agrupación
                var fase = new OrdenFase
                {
                    IdOrden = orden.IdOrden,
                    CodigoFase = linea.CodigoArticulo,
                    NumeroFase = 1,
                    CantidadEntrada = linea.Cantidad,
                    FechaFin = pedido.FechaEntrega,
                    Estado = nameof(EstadoOrden.Pendiente)
                };
                await _ordenFaseRepo.AddAsync(fase);
            }
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

                await CrearOrdenesAgrupacionAsync(pedido);

                MensajeInformacion.Mostrar("VENTAS",
                    $"Pedido #{pedido.IdPedido} creado correctamente. Total: {TotalPedido:0.00} €", 2);

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
        //  CERRAR PEDIDO — muestra diálogo de ubicación por cada artículo
        // ================================================================
        public async Task<bool> CerrarPedidoAsync(FilaPedido fila)
        {
            if (fila.Estado == "Entregado")
            { MensajeError.Mostrar("VENTAS", "Este pedido ya está entregado."); return false; }

            // Cargar el pedido completo con sus líneas
            var pedido = await _pedidoRepo.GetByIdAsync(fila.IdPedido);
            if (pedido == null)
            { MensajeError.Mostrar("VENTAS", "Pedido no encontrado."); return false; }

            var ubicacionesPorArticulo = new Dictionary<string, int>();

            foreach (var linea in pedido.Lineas)
            {
                // Obtener ubicaciones disponibles del artículo
                var ubicaciones = await _escandalloRepo.GetUbicacionesByArticuloAsync(
                    linea.Articulo?.IdArticulo ?? 0);

                if (ubicaciones.Count == 0)
                {
                    MensajeError.Mostrar("VENTAS",
                        $"El artículo {linea.CodigoArticulo} no tiene stock en ninguna ubicación. No se puede entregar el pedido.");
                    return false;
                }

                // Mostrar diálogo de selección de ubicación
                var dlg = new recTivo.Frontend.Dialogos.VentanasInicio.DialogoSeleccionUbicacion(
                    linea.CodigoArticulo,
                    linea.Articulo?.descrip ?? "",
                    linea.Cantidad,
                    ubicaciones)
                { Owner = System.Windows.Application.Current.MainWindow };

                bool? result = dlg.ShowDialog();

                if (dlg.Cancelado || result != true)
                {
                    MensajeError.Mostrar("VENTAS", "Operación cancelada.");
                    return false;
                }

                ubicacionesPorArticulo[linea.CodigoArticulo] = dlg.UbicacionElegida!.IdUbicacion;
            }

            try
            {
                await _pedidoRepo.CerrarPedidoAsync(fila.IdPedido, ubicacionesPorArticulo);

                MensajeInformacion.Mostrar("VENTAS",
                    $"Pedido #{fila.IdPedido} entregado. Stock descontado.", 2);

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