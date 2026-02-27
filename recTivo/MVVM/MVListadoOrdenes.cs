using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

namespace recTivo.MVVM
{
    public class MVListadoOrdenes : MVBase
    {
        private readonly OrdenRepository _ordenRepo;
        private readonly ArticuloRepository _articuloRepo;
        private readonly OrdenFaseRepository _ordenFaseRepo;

        public MVListadoOrdenes(
            OrdenRepository ordenRepo,
            ArticuloRepository articuloRepo,
            OrdenFaseRepository ordenFaseRepo)
        {
            _ordenRepo = ordenRepo;
            _articuloRepo = articuloRepo;
            _ordenFaseRepo = ordenFaseRepo;
        }

        // ── Lista completa cargada de BD ──────────────────────────────────
        private List<OrdenViewModel> _todasOrdenes = new();

        // ── Lista filtrada que ve el DataGrid ─────────────────────────────
        private ObservableCollection<OrdenViewModel> _ordenesFiltradas = new();
        public ObservableCollection<OrdenViewModel> OrdenesFiltradas
        {
            get => _ordenesFiltradas;
            set => SetProperty(ref _ordenesFiltradas, value);
        }

        // ── Orden seleccionada ────────────────────────────────────────────
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

        // ── Fases de la orden seleccionada ────────────────────────────────
        public ObservableCollection<OrdenFase> FasesOrden { get; } = new();

        private bool _fasesVisible;
        public bool FasesVisible
        {
            get => _fasesVisible;
            set => SetProperty(ref _fasesVisible, value);
        }

        // ── FILTROS ───────────────────────────────────────────────────────
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

        // ── Opciones de estado para el combo filtro ───────────────────────
        public List<string> OpcionesEstado { get; } = new()
            { "Todas", "Pendiente", "En curso", "Cerrada" };

        // ================================================================
        //   INICIALIZAR
        // ================================================================
        public async Task InicializarAsync()
        {
            await CargarOrdenesAsync();
        }

        public async Task CargarOrdenesAsync()
        {
            try
            {
                var ordenes = (await _ordenRepo.GetAllAsync()).ToList();
                var articulos = (await _articuloRepo.GetAllAsync()).ToList();

                _todasOrdenes = ordenes.Select(o =>
                {
                    var art = articulos.FirstOrDefault(a => a.IdArticulo == o.IdArticulo);
                    return new OrdenViewModel
                    {
                        Orden = o,
                        Descripcion = art?.descrip ?? ""
                    };
                })
                .OrderByDescending(o => o.Orden.IdOrden)
                .ToList();

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVListadoOrdenes] Error: {ex.Message}");
            }
        }

        // ================================================================
        //   FILTRAR
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
        //   CARGAR FASES DE LA ORDEN SELECCIONADA
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

                foreach (var f in fases)
                    FasesOrden.Add(f);

                FasesVisible = fases.Count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MVListadoOrdenes] Error fases: {ex.Message}");
            }
        }
    }

    // ── Wrapper para mostrar Orden + descripción del artículo ─────────────
    public class OrdenViewModel
    {
        public Orden Orden { get; set; } = null!;
        public string Descripcion { get; set; } = "";

        public int IdOrden => Orden.IdOrden;
        public string Codigo => Orden.Codigo;
        public int Cantidad => Orden.Cantidad;
        public string FechaFin => Orden.FechaFin?.ToString("dd/MM/yyyy") ?? "—";
        public string Estado => Orden.EstadoTexto;
        public string EstadoBD => Orden.Estado;
    }
}
