using recTivo.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Windows.Data;

namespace recTivo.MVVM
{
    public class MVArticulo : MVBase
    {
        private Articulo _articulo;

        private readonly ArticuloRepository _articuloRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly OrdenRepository _ordenRepository;

        public ListCollectionView? ArticulosView { get; private set; }

        public MVArticulo(
            ArticuloRepository articuloRepository,
            ClienteRepository clienteRepository,
            EmpleadoRepository empleadoRepository,
            OrdenRepository ordenRepository)
        {
            _articuloRepository = articuloRepository;
            _clienteRepository = clienteRepository;
            _empleadoRepository = empleadoRepository;
            _ordenRepository = ordenRepository;

            _articulo = new Articulo { Pvp = 0.00 };
        }

        // -----------------------------
        // PROPIEDADES PRINCIPALES
        // -----------------------------

        private List<Articulo>? _listaArticulos;
        public List<Articulo>? ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }

        private Articulo? _articuloSeleccionado;
        public Articulo? ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set => SetProperty(ref _articuloSeleccionado, value);
        }

        // -----------------------------
        // FILTROS CON CASCADA
        // -----------------------------

        private string? _filtroCodigo;
        public string? FiltroCodigo
        {
            get => _filtroCodigo;
            set
            {
                if (SetProperty(ref _filtroCodigo, value))
                {
                    FiltrarPorCodigo();
                    ArticulosView?.Refresh();
                }
            }
        }

        private string? _filtroDescripcion;
        public string? FiltroDescripcion
        {
            get => _filtroDescripcion;
            set
            {
                if (SetProperty(ref _filtroDescripcion, value))
                {
                    FiltrarPorDescripcion();
                    ArticulosView?.Refresh();
                }
            }
        }

        private string? _filtroDescripcion2;
        public string? FiltroDescripcion2
        {
            get => _filtroDescripcion2;
            set
            {
                if (SetProperty(ref _filtroDescripcion2, value))
                {
                    FiltrarPorDescripcion2();
                    ArticulosView?.Refresh();
                }
            }
        }

        // -----------------------------
        // LISTAS FILTRADAS PARA COMBOBOX
        // -----------------------------

        private List<string>? _codigosLista;
        public List<string>? CodigosLista
        {
            get => _codigosLista;
            set => SetProperty(ref _codigosLista, value);
        }

        private List<string>? _descripcionesLista;
        public List<string>? DescripcionesLista
        {
            get => _descripcionesLista;
            set => SetProperty(ref _descripcionesLista, value);
        }

        private List<string>? _descripciones2Lista;
        public List<string>? Descripciones2Lista
        {
            get => _descripciones2Lista;
            set => SetProperty(ref _descripciones2Lista, value);
        }

        // -----------------------------
        // MÉTODOS DE FILTRADO EN CASCADA
        // -----------------------------

        private void FiltrarPorCodigo()
        {
            if (ListaArticulos == null) return;

            if (string.IsNullOrWhiteSpace(FiltroCodigo))
            {
                RestablecerFiltros();
                return;
            }

            var articulosFiltrados = ListaArticulos
                .Where(a => a.Codigo == FiltroCodigo)
                .ToList();

            if (articulosFiltrados.Any())
            {
                if (articulosFiltrados.Count == 1)
                {
                    var articulo = articulosFiltrados.First();

                    _filtroDescripcion = articulo.descrip;
                    _filtroDescripcion2 = articulo.descrip2;

                    OnPropertyChanged(nameof(FiltroDescripcion));
                    OnPropertyChanged(nameof(FiltroDescripcion2));

                    ArticuloSeleccionado = articulo;
                }

                ActualizarListasFiltradas(articulosFiltrados);
            }
        }

        private void FiltrarPorDescripcion()
        {
            if (ListaArticulos == null) return;

            var query = ListaArticulos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroDescripcion))
                query = query.Where(a => a.descrip == FiltroDescripcion);

            if (!string.IsNullOrWhiteSpace(FiltroDescripcion2))
                query = query.Where(a => a.descrip2 == FiltroDescripcion2);

            if (!string.IsNullOrWhiteSpace(FiltroCodigo))
                query = query.Where(a => a.Codigo == FiltroCodigo);

            var articulosFiltrados = query.ToList();

            if (articulosFiltrados.Count == 1)
            {
                var articulo = articulosFiltrados.First();

                _filtroCodigo = articulo.Codigo;
                _filtroDescripcion2 = articulo.descrip2;

                OnPropertyChanged(nameof(FiltroCodigo));
                OnPropertyChanged(nameof(FiltroDescripcion2));

                ArticuloSeleccionado = articulo;
            }

            ActualizarListasFiltradas(articulosFiltrados);
        }

        private void FiltrarPorDescripcion2()
        {
            if (ListaArticulos == null) return;

            var query = ListaArticulos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroDescripcion2))
                query = query.Where(a => a.descrip2 == FiltroDescripcion2);

            if (!string.IsNullOrWhiteSpace(FiltroDescripcion))
                query = query.Where(a => a.descrip == FiltroDescripcion);

            if (!string.IsNullOrWhiteSpace(FiltroCodigo))
                query = query.Where(a => a.Codigo == FiltroCodigo);

            var articulosFiltrados = query.ToList();

            if (articulosFiltrados.Count == 1)
            {
                var articulo = articulosFiltrados.First();

                _filtroCodigo = articulo.Codigo;
                _filtroDescripcion = articulo.descrip;

                OnPropertyChanged(nameof(FiltroCodigo));
                OnPropertyChanged(nameof(FiltroDescripcion));

                ArticuloSeleccionado = articulo;
            }

            ActualizarListasFiltradas(articulosFiltrados);
        }

        private void ActualizarListasFiltradas(List<Articulo> articulosFiltrados)
        {
            CodigosLista = articulosFiltrados
                .Select(a => a.Codigo)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            DescripcionesLista = articulosFiltrados
                .Select(a => a.descrip)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            Descripciones2Lista = articulosFiltrados
                .Select(a => a.descrip2)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        private void RestablecerFiltros()
        {
            if (ListaArticulos == null) return;

            CodigosLista = ListaArticulos
                .Select(a => a.Codigo)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            DescripcionesLista = ListaArticulos
                .Select(a => a.descrip)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            Descripciones2Lista = ListaArticulos
                .Select(a => a.descrip2)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        // -----------------------------
        // FILTRO PARA ARTICULOS VIEW
        // -----------------------------

        private bool FiltrarArticulosEnVista(object obj)
        {
            if (obj is not Articulo art)
                return false;

            bool coincideCodigo =
                string.IsNullOrWhiteSpace(FiltroCodigo) ||
                (art.Codigo?.Contains(FiltroCodigo, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideDescrip =
                string.IsNullOrWhiteSpace(FiltroDescripcion) ||
                (art.descrip?.Contains(FiltroDescripcion, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideDescrip2 =
                string.IsNullOrWhiteSpace(FiltroDescripcion2) ||
                (art.descrip2?.Contains(FiltroDescripcion2, StringComparison.OrdinalIgnoreCase) ?? false);

            return coincideCodigo && coincideDescrip && coincideDescrip2;
        }

        // -----------------------------
        // INICIALIZACIÓN
        // -----------------------------

        public async Task Inicializa()
        {
            try
            {
                ListaArticulos = await _articuloRepository.GetAllWithUbicacionesAsync();

                RestablecerFiltros();

                ArticulosView = new ListCollectionView(ListaArticulos);
                ArticulosView.Filter = FiltrarArticulosEnVista;

                OnPropertyChanged(nameof(ArticulosView));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar datos\n{ex.Message}", 0);
            }
        }

        // -----------------------------
        // LIMPIAR FILTROS
        // -----------------------------

        public void LimpiarFiltros()
        {
            FiltroCodigo = null;
            FiltroDescripcion = null;
            FiltroDescripcion2 = null;
            ArticuloSeleccionado = null;
            RestablecerFiltros();
        }

        // -----------------------------
        // PROPIEDADES DEL ARTÍCULO
        // -----------------------------

        public string? Codigo
        {
            get => _articulo.Codigo;
            set
            {
                _articulo.Codigo = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ErrorCodigo));
                OnPropertyChanged(nameof(PuedeGuardar));
            }
        }

        public string? Descrip
        {
            get => _articulo.descrip;
            set
            {
                _articulo.descrip = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ErrorDescrip));
                OnPropertyChanged(nameof(PuedeGuardar));
            }
        }

        public string? Descrip2
        {
            get => _articulo.descrip2;
            set { _articulo.descrip2 = value; OnPropertyChanged(); }
        }

        public double? Pvp
        {
            get => _articulo.Pvp;
            set { _articulo.Pvp = value ?? 0.00; OnPropertyChanged(); }
        }

        public decimal? PrecioCompra
        {
            get => _articulo.PrecioCompra;
            set { _articulo.PrecioCompra = value; OnPropertyChanged(); }
        }

        public decimal? Stock
        {
            get => _articulo.Stock;
            set { _articulo.Stock = value; OnPropertyChanged(); }
        }

        public bool PuedeGuardar => !string.IsNullOrWhiteSpace(Codigo) && !string.IsNullOrWhiteSpace(Descrip);

        // -----------------------------
        // VALIDACIÓN (strings simples)
        // -----------------------------

        public string ErrorCodigo =>
            string.IsNullOrWhiteSpace(Codigo) ? "El código es obligatorio." : string.Empty;

        public string ErrorDescrip =>
            string.IsNullOrWhiteSpace(Descrip) ? "La descripción es obligatoria." : string.Empty;

        // -----------------------------
        // CRUD
        // -----------------------------

        public Task<bool> CargarArticuloSeleccionadoAsync()
        {
            if (ArticuloSeleccionado == null)
                return Task.FromResult(false);

            _articulo = ArticuloSeleccionado;

            OnPropertyChanged(nameof(Codigo));
            OnPropertyChanged(nameof(Descrip));
            OnPropertyChanged(nameof(Descrip2));
            OnPropertyChanged(nameof(Pvp));
            OnPropertyChanged(nameof(Stock));
            OnPropertyChanged(nameof(PrecioCompra));

            return Task.FromResult(true);
        }

        public async Task<bool> GuardarAsync()
        {
            try
            {
                await _articuloRepository.AddAsync(_articulo);
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al guardar artículo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ModificarAsync()
        {
            try
            {
                await _articuloRepository.UpdateAsync(_articulo);
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al modificar artículo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> BajaPorCodigoAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Codigo))
                    return false;

                var articulo = await _articuloRepository.GetByCodigoAsync(Codigo);
                if (articulo != null)
                {
                    _articuloRepository.Remove(articulo);
                    await _articuloRepository.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al dar de baja artículo: {ex.Message}");
                return false;
            }
        }
    }
}