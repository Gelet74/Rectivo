using recTivo.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace recTivo.MVVM
{
    public class FilaPTSeleccionado : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Articulo Articulo { get; set; } = null!;
        public string Codigo => Articulo.Codigo;
        public string Descripcion => Articulo.descrip ?? "";
        public string Descripcion2 => Articulo.descrip2 ?? "";
        public decimal Cantidad { get; set; } = 1;

        private bool _isMarcado;
        public bool IsMarcado
        {
            get => _isMarcado;
            set { _isMarcado = value; OnPropertyChanged(nameof(IsMarcado)); }
        }
    }


    public class FilaOrdenPreview
    {
        public string CodigoArticulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Descripcion2 { get; set; } = "";
        public decimal Cantidad { get; set; }
        public bool EsPT { get; set; }
        public bool EsNueva { get; set; } = true;
        public string Tipo => EsPT ? "PT" : "PS";
        public string AccionTexto => EsNueva ? "Nueva" : "Agrupar";
    }

    public class OrdenViewModel
    {
        public Orden Orden { get; set; } = null!;
        public string Descripcion { get; set; } = "";
        public string Descrip2 { get; set; } = "";
        public int IdOrden => Orden.IdOrden;
        public string Codigo => Orden.Codigo;
        public int Cantidad => Orden.Cantidad;
        public string FechaFin => Orden.FechaFin?.ToString("dd/MM/yyyy") ?? "—";
        public string Estado => Orden.EstadoTexto;
        public string EstadoBD => Orden.Estado;
    }

    public class MVOrden : MVBase
    {
        private readonly EscandalloRepository _escandalloRepo;
        private readonly ArticuloRepository _articuloRepo;
        private readonly OrdenRepository _ordenRepo;
        private readonly EmpleadoRepository _empleadoRepo;
        private readonly OrdenFaseRepository _ordenFaseRepo;

        // CORRECCIÓN 1: constructor sin RectivoContext (no se necesita en el ViewModel)
        public MVOrden(
            EscandalloRepository escandalloRepo,
            ArticuloRepository articuloRepo,
            OrdenRepository ordenRepo,
            EmpleadoRepository empleadoRepo,
            OrdenFaseRepository ordenFaseRepo)
        {
            _escandalloRepo = escandalloRepo;
            _articuloRepo = articuloRepo;
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _ordenFaseRepo = ordenFaseRepo;
        }

        // ================================================================
        //   ESTADO COMPARTIDO
        // ================================================================
        private List<Articulo> _todosArticulos = new();

        // ================================================================
        //   SECCIÓN: PROCESAR ORDEN (PT → PS)
        // ================================================================
        private List<Articulo> _articulosPT = new();
        public List<Articulo> ArticulosPT
        {
            get => _articulosPT;
            set => SetProperty(ref _articulosPT, value);
        }

        public ObservableCollection<FilaPTSeleccionado> PTSeleccionados { get; } = new();

        private DateTime? _fechaFin;
        public DateTime? FechaFin
        {
            get => _fechaFin;
            set => SetProperty(ref _fechaFin, value);
        }

        private bool _incluirPT = false;
        public bool IncluirPT
        {
            get => _incluirPT;
            set => SetProperty(ref _incluirPT, value);
        }

        public ObservableCollection<FilaOrdenPreview> OrdenesPreview { get; } = new();

        private bool _previewVisible;
        public bool PreviewVisible
        {
            get => _previewVisible;
            set => SetProperty(ref _previewVisible, value);
        }

        // ================================================================
        //   SECCIÓN: PS DIRECTO
        // ================================================================
        private List<Articulo> _articulosPS = new();

        private string _filtroBusquedaPS = "";
        public string FiltroBusquedaPS
        {
            get => _filtroBusquedaPS;
            set { SetProperty(ref _filtroBusquedaPS, value); AplicarFiltroPS(); }
        }

        private List<Articulo> _articulosPSFiltrados = new();
        public List<Articulo> ArticulosPSFiltrados
        {
            get => _articulosPSFiltrados;
            set => SetProperty(ref _articulosPSFiltrados, value);
        }

        public ObservableCollection<FilaPTSeleccionado> PSSeleccionados { get; } = new();

        private DateTime? _fechaFinPS;
        public DateTime? FechaFinPS
        {
            get => _fechaFinPS;
            set => SetProperty(ref _fechaFinPS, value);
        }

        public ObservableCollection<FilaOrdenPreview> OrdenesPSPreview { get; } = new();

        private bool _previewPSVisible;
        public bool PreviewPSVisible
        {
            get => _previewPSVisible;
            set => SetProperty(ref _previewPSVisible, value);
        }

        // ================================================================
        //   SECCIÓN: LISTADO DE ÓRDENES
        // ================================================================
        private List<OrdenViewModel> _todasOrdenes = new();

        private ObservableCollection<OrdenViewModel> _ordenesFiltradas = new();
        public ObservableCollection<OrdenViewModel> OrdenesFiltradas
        {
            get => _ordenesFiltradas;
            set => SetProperty(ref _ordenesFiltradas, value);
        }

        private OrdenViewModel? _ordenSeleccionada;
        public OrdenViewModel? OrdenSeleccionada
        {
            get => _ordenSeleccionada;
            set { SetProperty(ref _ordenSeleccionada, value); _ = CargarFasesAsync(); }
        }

        public ObservableCollection<OrdenFase> FasesOrden { get; } = new();

        private bool _fasesVisible;
        public bool FasesVisible
        {
            get => _fasesVisible;
            set => SetProperty(ref _fasesVisible, value);
        }

        private string? _filtroEstado;
        public string? FiltroEstado
        {
            get => _filtroEstado;
            set { SetProperty(ref _filtroEstado, value); AplicarFiltros(); }
        }

        private DateTime? _filtroFechaDesde;
        public DateTime? FiltroFechaDesde
        {
            get => _filtroFechaDesde;
            set { SetProperty(ref _filtroFechaDesde, value); AplicarFiltros(); }
        }

        private DateTime? _filtroFechaHasta;
        public DateTime? FiltroFechaHasta
        {
            get => _filtroFechaHasta;
            set { SetProperty(ref _filtroFechaHasta, value); AplicarFiltros(); }
        }

        private string? _filtroCodigo;
        public string? FiltroCodigo
        {
            get => _filtroCodigo;
            set { SetProperty(ref _filtroCodigo, value); AplicarFiltros(); }
        }

        public List<string> OpcionesEstado { get; } = new()
            { "Todas", "Pendiente", "En curso", "Cerrada" };

        // ================================================================
        //   INICIALIZAR
        // ================================================================
        public async Task InicializarProcesoAsync()
        {
            try
            {
                _todosArticulos = (await _articuloRepo.GetAllAsync()).ToList();

                var escandallos = await _escandalloRepo.GetAllAsync();
                var codigosConEscandallo = new HashSet<string>(
                    escandallos.Select(e => e.CodigoProducto));

                ArticulosPT = _todosArticulos
                    .Where(a => a.Codigo.StartsWith("PT")
                             && codigosConEscandallo.Contains(a.Codigo))
                    .OrderBy(a => a.Codigo)
                    .ToList();

                InicializarPS();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al inicializar: {ex.Message}");
            }
        }

        public async Task InicializarListadoAsync()
        {
            _todosArticulos = (await _articuloRepo.GetAllAsync()).ToList();
            await CargarOrdenesAsync();
        }

        private void InicializarPS()
        {
            _articulosPS = _todosArticulos
                .Where(a => a.Codigo.StartsWith("PS"))
                .OrderBy(a => a.Codigo)
                .ToList();
            ArticulosPSFiltrados = new List<Articulo>(_articulosPS);
        }

        private void AplicarFiltroPS()
        {
            if (string.IsNullOrWhiteSpace(FiltroBusquedaPS))
            {
                ArticulosPSFiltrados = new List<Articulo>(_articulosPS);
            }
            else
            {
                string filtro = FiltroBusquedaPS.Trim().ToLower();
                ArticulosPSFiltrados = _articulosPS
                    .Where(a => a.Codigo.ToLower().Contains(filtro) ||
                                (a.descrip?.ToLower().Contains(filtro) ?? false))
                    .ToList();
            }
        }

        // ================================================================
        //   PROCESAR PT: TOGGLE
        // ================================================================
        public void TogglePT(Articulo articulo, bool marcado)
        {
            if (marcado)
            {
                if (!PTSeleccionados.Any(p => p.Codigo == articulo.Codigo))
                {
                    PTSeleccionados.Add(new FilaPTSeleccionado
                    {
                        Articulo = articulo,
                        Cantidad = 1,
                        IsMarcado = true
                    });
                }
            }
            else
            {
                var fila = PTSeleccionados.FirstOrDefault(p => p.Codigo == articulo.Codigo);
                if (fila != null)
                    PTSeleccionados.Remove(fila);
            }

            OrdenesPreview.Clear();
            PreviewVisible = false;
        }



        // ================================================================
        //   PS DIRECTO: TOGGLE
        // ================================================================
        public void TogglePS(Articulo articulo, bool marcado)
        {
            if (marcado)
            {
                if (!PSSeleccionados.Any(p => p.Codigo == articulo.Codigo))
                    PSSeleccionados.Add(new FilaPTSeleccionado { Articulo = articulo, Cantidad = 1 });
            }
            else
            {
                var fila = PSSeleccionados.FirstOrDefault(p => p.Codigo == articulo.Codigo);
                if (fila != null) PSSeleccionados.Remove(fila);
            }
            OrdenesPSPreview.Clear();
            PreviewPSVisible = false;
        }

        // ================================================================
        //   PROCESAR PT: CALCULAR PREVIEW
        // ================================================================
        public async Task CalcularPreviewAsync()
        {
            OrdenesPreview.Clear();
            PreviewVisible = false;

            if (PTSeleccionados.Count == 0)
            { MensajeError.Mostrar("ÓRDENES", "Selecciona al menos un artículo PT."); return; }
            if (PTSeleccionados.Any(p => p.Cantidad <= 0))
            { MensajeError.Mostrar("ÓRDENES", "Todas las cantidades deben ser mayores que 0."); return; }
            if (FechaFin == null)
            { MensajeError.Mostrar("ÓRDENES", "Debes seleccionar una fecha fin."); return; }

            try
            {
                var acumulado = new Dictionary<string, decimal>();

                foreach (var filaPT in PTSeleccionados)
                {
                    var escandallo = await _escandalloRepo.GetByCodigoProductoAsync(filaPT.Codigo);
                    if (escandallo == null) continue;

                    var componentes = await _escandalloRepo
                        .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                    await RecopilarPS(componentes, filaPT.Cantidad, acumulado);

                    if (IncluirPT)
                    {
                        OrdenesPreview.Add(new FilaOrdenPreview
                        {
                            CodigoArticulo = filaPT.Codigo,
                            Descripcion = filaPT.Descripcion,
                            Descripcion2 = filaPT.Descripcion2,
                            Cantidad = filaPT.Cantidad,
                            EsPT = true,
                            EsNueva = true
                        });
                    }
                }

                if (acumulado.Count == 0)
                {
                    MensajeError.Mostrar("ÓRDENES",
                        "Los escandallos seleccionados no contienen componentes PS.");
                    return;
                }

                foreach (var kvp in acumulado.OrderBy(k => k.Key))
                {
                    var art = _todosArticulos.FirstOrDefault(a => a.Codigo == kvp.Key);
                    OrdenesPreview.Add(new FilaOrdenPreview
                    {
                        CodigoArticulo = kvp.Key,
                        Descripcion = art?.descrip ?? "",
                        Descripcion2 = art?.descrip2 ?? "",
                        Cantidad = kvp.Value,
                        EsPT = false,
                        EsNueva = true
                    });
                }

                PreviewVisible = true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al calcular preview: {ex.Message}");
            }
        }

        // ================================================================
        //   PS DIRECTO: CALCULAR PREVIEW
        // ================================================================
        public async Task CalcularPreviewPSAsync()
        {
            OrdenesPSPreview.Clear();
            PreviewPSVisible = false;

            if (PSSeleccionados.Count == 0)
            { MensajeError.Mostrar("ÓRDENES PS", "Selecciona al menos un artículo PS."); return; }
            if (PSSeleccionados.Any(p => p.Cantidad <= 0))
            { MensajeError.Mostrar("ÓRDENES PS", "Todas las cantidades deben ser mayores que 0."); return; }
            if (FechaFinPS == null)
            { MensajeError.Mostrar("ÓRDENES PS", "Debes seleccionar una fecha fin."); return; }

            foreach (var fila in PSSeleccionados)
            {
                OrdenesPSPreview.Add(new FilaOrdenPreview
                {
                    CodigoArticulo = fila.Codigo,
                    Descripcion = fila.Descripcion,
                    Descripcion2 = fila.Descripcion2,
                    Cantidad = fila.Cantidad,
                    EsPT = false,
                    EsNueva = true
                });
            }

            PreviewPSVisible = true;
            await Task.CompletedTask;
        }

        // ================================================================
        //   PROCESAR PT: RECOPILAR PS RECURSIVAMENTE
        // ================================================================
        private async Task RecopilarPS(
            List<ComponenteEscandallo> componentes,
            decimal factorCantidad,
            Dictionary<string, decimal> acumulado)
        {
            foreach (var comp in componentes)
            {
                decimal cantidadReal = (comp.Cantidad ?? 1) * factorCantidad;

                if (comp.CodigoArticulo.StartsWith("PS"))
                {
                    if (acumulado.ContainsKey(comp.CodigoArticulo))
                        acumulado[comp.CodigoArticulo] += cantidadReal;
                    else
                        acumulado[comp.CodigoArticulo] = cantidadReal;
                }
                else
                {
                    var subEsc = await _escandalloRepo.GetByCodigoProductoAsync(comp.CodigoArticulo);
                    if (subEsc != null)
                    {
                        var subComps = await _escandalloRepo
                            .GetComponentesByEscandalloAsync(subEsc.IdEscandallo);
                        await RecopilarPS(subComps, cantidadReal, acumulado);
                    }
                }
            }
        }

        // ================================================================
        //   PROCESAR PT: CONFIRMAR Y GENERAR ÓRDENES
        // ================================================================
        public async Task<bool> GenerarOrdenesAsync(Empleado empleadoActual)
        {
            if (OrdenesPreview.Count == 0)
            { MensajeError.Mostrar("ÓRDENES", "Calcula el preview primero."); return false; }

            bool tienePermiso = empleadoActual?.Rol?.Permisos
                .Any(p => p.NombrePermiso.ToLower().Contains("orden")) ?? false;

            if (!tienePermiso && (empleadoActual?.Rol?.Permisos.Count ?? 0) > 0)
            {
                MensajeError.Mostrar("ÓRDENES",
                    $"'{empleadoActual!.NombreCompleto}' no tiene permiso para generar órdenes.");
                return false;
            }

            try
            {
                int nuevas = 0;

                foreach (var fila in OrdenesPreview)
                {
                    int cantidadInt = (int)Math.Ceiling(fila.Cantidad);
                    var articuloId = _todosArticulos
                        .FirstOrDefault(a => a.Codigo == fila.CodigoArticulo)?.IdArticulo ?? 0;

                    var nuevaOrden = new Orden
                    {
                        Codigo = fila.CodigoArticulo,
                        Cantidad = cantidadInt,
                        FechaFin = FechaFin,
                        IdEmpleado = empleadoActual!.Id,
                        IdArticulo = articuloId,
                        Estado = nameof(EstadoOrden.Pendiente)
                    };

                    await _ordenRepo.AddAsync(nuevaOrden);
                    nuevas++;

                    if (nuevaOrden.Codigo.StartsWith("PS"))
                        await GenerarFasesAsync(nuevaOrden);
                    else if (nuevaOrden.Codigo.StartsWith("PT"))
                        await GenerarFaseAgrupamientoAsync(nuevaOrden);
                }

                MensajeInformacion.Mostrar("ÓRDENES", $"Se han generado {nuevas} órdenes de fabricación.", 2);

                PTSeleccionados.Clear();
                FechaFin = null;
                IncluirPT = false;
                OrdenesPreview.Clear();
                PreviewVisible = false;
                OnPropertyChanged(nameof(IncluirPT));

                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al guardar las órdenes: {ex.Message}");
                return false;
            }
        }

        // ================================================================
        //   PS DIRECTO: CONFIRMAR Y GENERAR ÓRDENES
        // ================================================================
        public async Task<bool> GenerarOrdenesPSAsync(Empleado empleadoActual)
        {
            if (OrdenesPSPreview.Count == 0)
            { MensajeError.Mostrar("ÓRDENES PS", "Calcula el preview primero."); return false; }

            bool tienePermiso = empleadoActual?.Rol?.Permisos
                .Any(p => p.NombrePermiso.ToLower().Contains("orden")) ?? false;

            if (!tienePermiso && (empleadoActual?.Rol?.Permisos.Count ?? 0) > 0)
            {
                MensajeError.Mostrar("ÓRDENES PS",
                    $"'{empleadoActual!.NombreCompleto}' no tiene permiso para generar órdenes.");
                return false;
            }

            try
            {
                int nuevas = 0;

                foreach (var fila in OrdenesPSPreview)
                {
                    int cantidadInt = (int)Math.Ceiling(fila.Cantidad);
                    var articuloId = _todosArticulos
                        .FirstOrDefault(a => a.Codigo == fila.CodigoArticulo)?.IdArticulo ?? 0;

                    var nuevaOrden = new Orden
                    {
                        Codigo = fila.CodigoArticulo,
                        Cantidad = cantidadInt,
                        FechaFin = FechaFinPS,
                        IdEmpleado = empleadoActual!.Id,
                        IdArticulo = articuloId,
                        Estado = nameof(EstadoOrden.Pendiente)
                    };

                    await _ordenRepo.AddAsync(nuevaOrden);
                    nuevas++;

                    await GenerarFasesAsync(nuevaOrden);
                }

                MensajeInformacion.Mostrar("ÓRDENES PS",
                    $"Se han generado {nuevas} órdenes PS de fabricación.", 2);

                PSSeleccionados.Clear();
                FechaFinPS = null;
                OrdenesPSPreview.Clear();
                PreviewPSVisible = false;

                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES PS", $"Error al guardar las órdenes: {ex.Message}");
                return false;
            }
        }

        // ================================================================
        //   FASES: GENERAR FASE DE AGRUPAMIENTO (PT)
        // ================================================================
        private async Task GenerarFaseAgrupamientoAsync(Orden ordenPT)
        {
            try
            {
                await _ordenFaseRepo.AddAsync(new OrdenFase
                {
                    IdOrden = ordenPT.IdOrden,
                    CodigoFase = "AGRUPAMIENTO",
                    NumeroFase = 1,
                    CantidadEntrada = ordenPT.Cantidad,
                    FechaFin = ordenPT.FechaFin,
                    Estado = nameof(EstadoOrden.Pendiente)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVOrden] Error generando fase agrupamiento: {ex.Message}");
            }
        }

        // ================================================================
        //   FASES: GENERAR FASES PS (máquinas)
        // ================================================================
        private async Task GenerarFasesAsync(Orden ordenPS)
        {
            try
            {
                var escandallo = await _escandalloRepo.GetByCodigoProductoAsync(ordenPS.Codigo);
                if (escandallo == null) return;

                var componentes = await _escandalloRepo
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                var fases = new List<(int numero, string codigo)>();
                await RecopilarFases(componentes, fases);

                if (fases.Count == 0) return;

                var fasesOrdenadas = fases.OrderBy(f => f.numero).ToList();
                var fechas = CalcularFechasFases(fasesOrdenadas, ordenPS.FechaFin);

                for (int i = 0; i < fasesOrdenadas.Count; i++)
                {
                    var (numero, codigo) = fasesOrdenadas[i];
                    await _ordenFaseRepo.AddAsync(new OrdenFase
                    {
                        IdOrden = ordenPS.IdOrden,
                        CodigoFase = codigo,
                        NumeroFase = i + 1,
                        CantidadEntrada = i == 0 ? ordenPS.Cantidad : 0,
                        FechaFin = fechas[i],
                        Estado = nameof(EstadoOrden.Pendiente)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVOrden] Error generando fases: {ex.Message}");
            }
        }

        private List<DateTime?> CalcularFechasFases(
            List<(int numero, string codigo)> fases, DateTime? fechaFin)
        {
            var resultado = new List<DateTime?>();
            if (fechaFin == null) { foreach (var _ in fases) resultado.Add(null); return resultado; }

            DateTime hoy = DateTime.Today;
            DateTime fin = fechaFin.Value.Date;
            int diasLaborables = ContarDiasLaborables(hoy, fin);

            if (diasLaborables <= 0) { foreach (var _ in fases) resultado.Add(fin); return resultado; }

            bool tiene01 = fases.Any(f => f.numero == 1);
            bool tiene02 = fases.Any(f => f.numero == 2);
            bool tiene03 = fases.Any(f => f.numero == 3);

            var pesos = new Dictionary<int, double>();
            if (tiene01 && tiene02 && tiene03) { pesos[1] = 0.20; pesos[2] = 0.50; pesos[3] = 1.0; }
            else if (tiene01 && tiene02) { pesos[1] = 0.40; pesos[2] = 1.0; }
            else if (tiene01 && tiene03) { pesos[1] = 0.29; pesos[3] = 1.0; }
            else { foreach (var f in fases) pesos[f.numero] = 1.0; }

            foreach (var (numero, _) in fases)
            {
                if (numero == fases.Last().numero)
                    resultado.Add(fin);
                else
                {
                    double peso = pesos.ContainsKey(numero) ? pesos[numero] : 1.0;
                    resultado.Add(SumarDiasLaborables(hoy, (int)Math.Round(diasLaborables * peso)));
                }
            }
            return resultado;
        }

        private int ContarDiasLaborables(DateTime desde, DateTime hasta)
        {
            int count = 0;
            DateTime d = desde.AddDays(1);
            while (d <= hasta)
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) count++;
                d = d.AddDays(1);
            }
            return count;
        }

        private DateTime SumarDiasLaborables(DateTime desde, int dias)
        {
            DateTime d = desde;
            int sumados = 0;
            while (sumados < dias)
            {
                d = d.AddDays(1);
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) sumados++;
            }
            return d;
        }

        // ================================================================
        //   FASES: RECOPILAR
        //   CORRECCIÓN 3: solo "01", "02", "03" exactos como fases
        //   para evitar que códigos como "013410BB" sean tratados como fase
        // ================================================================
        private async Task RecopilarFases(
            List<ComponenteEscandallo> componentes,
            List<(int numero, string codigo)> fases)
        {
            foreach (var comp in componentes)
            {
                string cod = comp.CodigoArticulo;

                bool esFase = cod == "01" || cod == "02" || cod == "03";

                if (esFase)
                {
                    int numero = int.Parse(cod);
                    if (!fases.Any(f => f.numero == numero))
                        fases.Add((numero, cod));
                }
                else
                {
                    var subEsc = await _escandalloRepo.GetByCodigoProductoAsync(cod);
                    if (subEsc != null)
                    {
                        var subComps = await _escandalloRepo
                            .GetComponentesByEscandalloAsync(subEsc.IdEscandallo);
                        await RecopilarFases(subComps, fases);
                    }
                }
            }
        }

        // ================================================================
        //   LISTADO: CARGAR ÓRDENES
        // ================================================================
        public async Task CargarOrdenesAsync()
        {
            try
            {
                var ordenes = (await _ordenRepo.GetAllAsync()).ToList();

                _todasOrdenes = ordenes.Select(o =>
                {
                    var art = _todosArticulos.FirstOrDefault(a => a.IdArticulo == o.IdArticulo);
                    return new OrdenViewModel
                    {
                        Orden = o,
                        Descripcion = art?.descrip ?? "",
                        Descrip2 = art?.descrip2 ?? ""
                    };
                })
                .OrderByDescending(o => o.Orden.IdOrden)
                .ToList();

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVOrden] Error cargando órdenes: {ex.Message}");
            }
        }

        // ================================================================
        //   LISTADO: FILTROS
        // ================================================================
        private void AplicarFiltros()
        {
            var resultado = _todasOrdenes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroEstado) && FiltroEstado != "Todas")
            {
                string estadoBD = FiltroEstado == "En curso" ? "EnCurso" : FiltroEstado;
                resultado = resultado.Where(o => o.Orden.Estado == estadoBD);
            }

            if (!string.IsNullOrWhiteSpace(FiltroCodigo))
                resultado = resultado.Where(o =>
                    o.Orden.Codigo.Contains(FiltroCodigo, StringComparison.OrdinalIgnoreCase));

            if (FiltroFechaDesde.HasValue)
                resultado = resultado.Where(o =>
                    o.Orden.FechaFin.HasValue &&
                    o.Orden.FechaFin.Value.Date >= FiltroFechaDesde.Value.Date);

            if (FiltroFechaHasta.HasValue)
                resultado = resultado.Where(o =>
                    o.Orden.FechaFin.HasValue &&
                    o.Orden.FechaFin.Value.Date <= FiltroFechaHasta.Value.Date);

            OrdenesFiltradas = new ObservableCollection<OrdenViewModel>(resultado);
        }

        public void LimpiarFiltros()
        {
            _filtroEstado = null;
            _filtroFechaDesde = null;
            _filtroFechaHasta = null;
            _filtroCodigo = null;
            OnPropertyChanged(nameof(FiltroEstado));
            OnPropertyChanged(nameof(FiltroFechaDesde));
            OnPropertyChanged(nameof(FiltroFechaHasta));
            OnPropertyChanged(nameof(FiltroCodigo));
            AplicarFiltros();
        }

        // ================================================================
        //   LISTADO: CARGAR FASES DE LA ORDEN SELECCIONADA
        //   CORRECCIÓN 4: refresca el estado de la orden desde BD
        // ================================================================
        private async Task CargarFasesAsync()
        {
            FasesOrden.Clear();
            FasesVisible = false;

            if (OrdenSeleccionada == null) return;

            try
            {
                // Recargar el estado de la orden desde BD para evitar caché de EF
                var ordenActualizada = await _ordenRepo.GetByIdAsync(OrdenSeleccionada.Orden.IdOrden);
                if (ordenActualizada != null)
                    OrdenSeleccionada.Orden.Estado = ordenActualizada.Estado;

                var fases = await _ordenFaseRepo
                    .GetByOrdenAsync(OrdenSeleccionada.Orden.IdOrden);

                foreach (var f in fases) FasesOrden.Add(f);
                FasesVisible = fases.Count > 0;

                FaseActiva = FasesOrden.FirstOrDefault(f => f.Estado == nameof(EstadoOrden.Pendiente));

                EsUltimaFase = FaseActiva != null &&
                               FaseActiva.NumeroFase == FasesOrden.Max(f => f.NumeroFase);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVOrden] Error fases: {ex.Message}");
            }
        }

        // ================================================================
        //   CERRAR FASE
        // ================================================================
        private OrdenFase? _faseActiva;
        public OrdenFase? FaseActiva
        {
            get => _faseActiva;
            set
            {
                SetProperty(ref _faseActiva, value);
                OnPropertyChanged(nameof(HayFaseActiva));
                OnPropertyChanged(nameof(PuedeCerrarFase));
            }
        }

        public bool HayFaseActiva => FaseActiva != null;

        public string NombreFaseActiva
        {
            get
            {
                if (FaseActiva == null) return "";
                string cod = FaseActiva.CodigoFase;
                string prefijo = cod.Length >= 2 ? cod.Substring(0, 2) : cod;
                if (FaseActiva.CodigoFase == "AGRUPAMIENTO")
                    return "FASE 1 · AGRUPAMIENTO";

                return prefijo switch
                {
                    "01" => "FASE 1 · SECCIONADORA",
                    "02" => "FASE 2 · CANTEADORA",
                    "03" => "FASE 3 · MECANIZADO",
                    _ => $"FASE {FaseActiva.NumeroFase}"
                };
            }
        }

        public string TextoBotonCerrar => EsUltimaFase ? "Cerrar fase y orden" : "Cerrar fase";

        private string _cierreCantidadOK = "";
        public string CierreCantidadOK
        {
            get => _cierreCantidadOK;
            set { SetProperty(ref _cierreCantidadOK, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        private string _cierreCantidadDefecto = "";
        public string CierreCantidadDefecto
        {
            get => _cierreCantidadDefecto;
            set { SetProperty(ref _cierreCantidadDefecto, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        private DateTime? _cierreFecha;
        public DateTime? CierreFecha
        {
            get => _cierreFecha;
            set { SetProperty(ref _cierreFecha, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        private bool _esUltimaFase;
        public bool EsUltimaFase
        {
            get => _esUltimaFase;
            set
            {
                SetProperty(ref _esUltimaFase, value);
                OnPropertyChanged(nameof(PuedeCerrarFase));
                OnPropertyChanged(nameof(ErrorCierreFase));
                OnPropertyChanged(nameof(TextoBotonCerrar));
            }
        }

        private string _ubicacionPasillo = "";
        public string UbicacionPasillo
        {
            get => _ubicacionPasillo;
            set { SetProperty(ref _ubicacionPasillo, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        private string _ubicacionEstanteria = "";
        public string UbicacionEstanteria
        {
            get => _ubicacionEstanteria;
            set { SetProperty(ref _ubicacionEstanteria, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        private string _ubicacionHueco = "";
        public string UbicacionHueco
        {
            get => _ubicacionHueco;
            set { SetProperty(ref _ubicacionHueco, value); OnPropertyChanged(nameof(PuedeCerrarFase)); OnPropertyChanged(nameof(ErrorCierreFase)); }
        }

        public bool PuedeCerrarFase =>
            FaseActiva != null &&
            int.TryParse(CierreCantidadOK, out int ok) && ok >= 0 &&
            int.TryParse(CierreCantidadDefecto, out int def) && def >= 0 &&
            (ok + def) <= FaseActiva.CantidadEntrada &&
            CierreFecha.HasValue &&
            (!EsUltimaFase || (
                !string.IsNullOrWhiteSpace(UbicacionPasillo) &&
                int.TryParse(UbicacionEstanteria, out _) &&
                int.TryParse(UbicacionHueco, out _)
            ));

        public string ErrorCierreFase
        {
            get
            {
                if (FaseActiva == null) return "";
                if (!int.TryParse(CierreCantidadOK, out int ok) || ok < 0)
                    return "CantidadOK debe ser ≥ 0";
                if (!int.TryParse(CierreCantidadDefecto, out int def) || def < 0)
                    return "Defectos debe ser ≥ 0";
                if (ok + def > FaseActiva.CantidadEntrada)
                    return $"OK + Defectos ({ok + def}) no puede superar la entrada ({FaseActiva.CantidadEntrada})";
                if (!CierreFecha.HasValue)
                    return "Introduce la fecha de cierre";
                if (EsUltimaFase)
                {
                    if (string.IsNullOrWhiteSpace(UbicacionPasillo))
                        return "Introduce el pasillo de ubicación";
                    if (!int.TryParse(UbicacionEstanteria, out _))
                        return "La estantería debe ser un número";
                    if (!int.TryParse(UbicacionHueco, out _))
                        return "El hueco debe ser un número";
                }
                return "";
            }
        }

        public async Task<bool> CerrarFaseActivaAsync(Empleado empleadoActual)
        {
            if (!PuedeCerrarFase) return false;

            int ok = int.Parse(CierreCantidadOK);
            int def = int.Parse(CierreCantidadDefecto);

            string? pasillo = EsUltimaFase ? UbicacionPasillo.Trim().ToUpper() : null;
            int? estanteria = EsUltimaFase && int.TryParse(UbicacionEstanteria, out int est) ? est : null;
            int? hueco = EsUltimaFase && int.TryParse(UbicacionHueco, out int hue) ? hue : null;

            try
            {
                await _ordenFaseRepo.CerrarFaseAsync(
                    FaseActiva!.IdOrdenFase,
                    ok, def,
                    empleadoActual.Id,
                    CierreFecha!.Value,
                    pasillo, estanteria, hueco);

                // CORRECCIÓN 4: recargar desde BD para obtener estado actualizado sin caché
                var ordenActualizada = await _ordenRepo.GetByIdAsync(OrdenSeleccionada!.Orden.IdOrden);
                string msg = $"{NombreFaseActiva} cerrada. Pasan a siguiente fase: {ok} unidades.";

                if (ordenActualizada?.Estado == nameof(EstadoOrden.Cerrada))
                    msg = $"Todas las fases completadas. Orden cerrada. {ok} uds de " +
                          $"{OrdenSeleccionada.Codigo} subidas a stock en {pasillo}-{estanteria}-{hueco}.";

                MensajeInformacion.Mostrar("FASE CERRADA", msg, 3);

                LimpiarCamposCierre();
                await CargarFasesAsync();
                await CargarOrdenesAsync();

                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", ex.Message);
                return false;
            }
        }


        private void LimpiarCamposCierre()
        {
            CierreCantidadOK = "";
            CierreCantidadDefecto = "";
            CierreFecha = null;
            UbicacionPasillo = "";
            UbicacionEstanteria = "";
            UbicacionHueco = "";
            EsUltimaFase = false;
        }
    }
}