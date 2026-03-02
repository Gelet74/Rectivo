using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

namespace recTivo.MVVM
{
    // ── Fila en la tabla de PT seleccionados ─────────────────────────────
    public class FilaPTSeleccionado
    {
        public Articulo Articulo { get; set; } = null!;
        public string Codigo => Articulo.Codigo;
        public string Descripcion => Articulo.descrip ?? "";
        public decimal Cantidad { get; set; } = 1;
    }

    // ── Fila en el preview de órdenes a generar ───────────────────────────
    public class FilaOrdenPreview
    {
        public string CodigoArticulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Cantidad { get; set; }
        public bool EsPT { get; set; }
        public bool EsNueva { get; set; } = true;

        public string Tipo => EsPT ? "PT" : "PS";
        public string AccionTexto => EsNueva ? "Nueva" : "Agrupar";
    }

    // ── Wrapper para el listado ───────────────────────────────────────────
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
        private readonly RectivoContext _context;

        public MVOrden(
            EscandalloRepository escandalloRepo,
            ArticuloRepository articuloRepo,
            OrdenRepository ordenRepo,
            EmpleadoRepository empleadoRepo,
            OrdenFaseRepository ordenFaseRepo,
            RectivoContext context)
        {
            _escandalloRepo = escandalloRepo;
            _articuloRepo = articuloRepo;
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
            _ordenFaseRepo = ordenFaseRepo;
            _context = context;
        }

        // ================================================================
        //   ESTADO COMPARTIDO
        // ================================================================
        private List<Articulo> _todosArticulos = new();

        // ================================================================
        //   SECCIÓN: PROCESAR ORDEN
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
            set
            {
                SetProperty(ref _ordenSeleccionada, value);
                _ = CargarFasesAsync();
            }
        }

        public ObservableCollection<OrdenFase> FasesOrden { get; } = new();

        private bool _fasesVisible;
        public bool FasesVisible
        {
            get => _fasesVisible;
            set => SetProperty(ref _fasesVisible, value);
        }

        // ── Filtros listado ───────────────────────────────────────────────
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
        //   INICIALIZAR (carga datos para ambas secciones)
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

        // ================================================================
        //   PROCESAR: TOGGLE PT
        // ================================================================
        public void TogglePT(Articulo articulo, bool marcado)
        {
            if (marcado)
            {
                if (!PTSeleccionados.Any(p => p.Codigo == articulo.Codigo))
                    PTSeleccionados.Add(new FilaPTSeleccionado { Articulo = articulo, Cantidad = 1 });
            }
            else
            {
                var fila = PTSeleccionados.FirstOrDefault(p => p.Codigo == articulo.Codigo);
                if (fila != null) PTSeleccionados.Remove(fila);
            }

            OrdenesPreview.Clear();
            PreviewVisible = false;
        }

        // ================================================================
        //   PROCESAR: CALCULAR PREVIEW
        // ================================================================
        public async Task CalcularPreviewAsync()
        {
            OrdenesPreview.Clear();
            PreviewVisible = false;

            if (PTSeleccionados.Count == 0)
            {
                MensajeError.Mostrar("ÓRDENES", "Selecciona al menos un artículo PT.");
                return;
            }
            if (PTSeleccionados.Any(p => p.Cantidad <= 0))
            {
                MensajeError.Mostrar("ÓRDENES", "Todas las cantidades deben ser mayores que 0.");
                return;
            }
            if (FechaFin == null)
            {
                MensajeError.Mostrar("ÓRDENES", "Debes seleccionar una fecha fin.");
                return;
            }

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
        //   PROCESAR: RECOPILAR PS RECURSIVAMENTE
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
        //   PROCESAR: CONFIRMAR Y GENERAR ÓRDENES
        // ================================================================
        public async Task<bool> GenerarOrdenesAsync(Empleado empleadoActual)
        {
            if (OrdenesPreview.Count == 0)
            {
                MensajeError.Mostrar("ÓRDENES", "Calcula el preview primero.");
                return false;
            }

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
                }

                MensajeInformacion.Mostrar("ÓRDENES",
                    $"Se han generado {nuevas} órdenes de fabricación.", 2);

                // Reset
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
        //   FASES: GENERAR
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
                System.Diagnostics.Debug.WriteLine(
                    $"[MVOrden] Error generando fases: {ex.Message}");
            }
        }

        private List<DateTime?> CalcularFechasFases(
            List<(int numero, string codigo)> fases,
            DateTime? fechaFin)
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

        private async Task RecopilarFases(
            List<ComponenteEscandallo> componentes,
            List<(int numero, string codigo)> fases)
        {
            foreach (var comp in componentes)
            {
                string cod = comp.CodigoArticulo;
                bool esFase = (cod.StartsWith("01") || cod.StartsWith("02") || cod.StartsWith("03"))
                              && cod.Length > 2;

                if (esFase)
                {
                    int numero = int.Parse(cod.Substring(0, 2));
                    if (!fases.Any(f => f.codigo == cod))
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
        // ================================================================
        private async Task CargarFasesAsync()
        {
            FasesOrden.Clear();
            FasesVisible = false;

            if (OrdenSeleccionada == null) return;

            try
            {
                var fases = await _ordenFaseRepo
                    .GetByOrdenAsync(OrdenSeleccionada.Orden.IdOrden);

                foreach (var f in fases) FasesOrden.Add(f);
                FasesVisible = fases.Count > 0;

                // Detectar fase activa (primera Pendiente)
                FaseActiva = FasesOrden.FirstOrDefault(f => f.Estado == nameof(EstadoOrden.Pendiente));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVOrden] Error fases: {ex.Message}");
            }
        }

        // ================================================================
        //   CERRAR FASE
        // ================================================================

        // ── Fase activa (siguiente pendiente) ─────────────────────────────
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

        public string NombreFaseActiva => FaseActiva?.CodigoFase.Substring(0, 2) switch
        {
            "01" => "FASE 1 · SECCIONADORA",
            "02" => "FASE 2 · CANTEADORA",
            "03" => "FASE 3 · MECANIZADO",
            _ => FaseActiva != null ? $"FASE {FaseActiva.NumeroFase}" : ""
        };

        // ── Campos de cierre ───────────────────────────────────────────────
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

        public bool PuedeCerrarFase =>
            FaseActiva != null &&
            int.TryParse(CierreCantidadOK, out int ok) && ok >= 0 &&
            int.TryParse(CierreCantidadDefecto, out int def) && def >= 0 &&
            (ok + def) <= FaseActiva.CantidadEntrada &&
            CierreFecha.HasValue;

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
                return "";
            }
        }

        public async Task<bool> CerrarFaseActivaAsync(Empleado empleadoActual)
        {
            if (!PuedeCerrarFase) return false;

            int ok = int.Parse(CierreCantidadOK);
            int def = int.Parse(CierreCantidadDefecto);

            try
            {
                await _ordenFaseRepo.CerrarFaseAsync(
                    FaseActiva!.IdOrdenFase,
                    ok, def,
                    empleadoActual.Id,
                    CierreFecha!.Value,
                    _context);

                string msg = $"{NombreFaseActiva} cerrada. Pasan a siguiente fase: {ok} unidades.";

                // Comprobar si era la última — la orden se habrá cerrado
                var ordenActualizada = await _ordenRepo.GetByIdAsync(OrdenSeleccionada!.Orden.IdOrden);
                if (ordenActualizada?.Estado == nameof(EstadoOrden.Cerrada))
                    msg = $"Todas las fases completadas. Orden cerrada. {ok} uds de {OrdenSeleccionada.Codigo} subidas a stock.";

                MensajeInformacion.Mostrar("FASE CERRADA", msg, 3);

                // Limpiar campos y recargar
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
        }
    }
}
