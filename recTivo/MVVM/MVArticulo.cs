using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
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
        private readonly RectivoContext _context;

        public ListCollectionView ArticulosView { get; private set; }

        public MVArticulo(
            ArticuloRepository articuloRepository,
            ClienteRepository clienteRepository,
            EmpleadoRepository empleadoRepository,
            OrdenRepository ordenRepository,
            RectivoContext context)
        {
            _articuloRepository = articuloRepository;
            _clienteRepository = clienteRepository;
            _empleadoRepository = empleadoRepository;
            _ordenRepository = ordenRepository;
            _context = context;
            _articulo = new Articulo();
        }

        // -----------------------------
        // PROPIEDADES PRINCIPALES
        // -----------------------------

        private List<Articulo> _listaArticulos;
        public List<Articulo> ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }

        private Articulo _articuloSeleccionado;
        public Articulo ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set => SetProperty(ref _articuloSeleccionado, value);
        }

        // -----------------------------
        // FILTROS (los que usa el XAML)
        // -----------------------------

        private string _filtroCodigo;
        public string FiltroCodigo
        {
            get => _filtroCodigo;
            set
            {
                if (SetProperty(ref _filtroCodigo, value))
                    ArticulosView?.Refresh();
            }
        }

        private string _filtroDescripcion;
        public string FiltroDescripcion
        {
            get => _filtroDescripcion;
            set
            {
                if (SetProperty(ref _filtroDescripcion, value))
                    ArticulosView?.Refresh();
            }
        }

        private string _filtroDescripcion2;
        public string FiltroDescripcion2
        {
            get => _filtroDescripcion2;
            set
            {
                if (SetProperty(ref _filtroDescripcion2, value))
                    ArticulosView?.Refresh();
            }
        }

        // -----------------------------
        // LISTAS PARA COMBOBOX (si quieres sugerencias)
        // -----------------------------

        public List<string> CodigosLista => ListaArticulos?
            .Select(a => a.Codigo)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        public List<string> DescripcionesLista => ListaArticulos?
            .Select(a => a.Descrip)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        public List<string> Descripciones2Lista => ListaArticulos?
            .Select(a => a.Descrip2)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // -----------------------------
        // INICIALIZACIÓN
        // -----------------------------

        public async Task Inicializa()
        {
            try
            {
                ListaArticulos = (await _articuloRepository.GetAllAsync())
                    .OrderBy(a => a.Codigo)
                    .ToList();

                ArticulosView = new ListCollectionView(ListaArticulos);
                ArticulosView.Filter = FiltrarArticulos;

                OnPropertyChanged(nameof(ArticulosView));
                OnPropertyChanged(nameof(CodigosLista));
                OnPropertyChanged(nameof(DescripcionesLista));
                OnPropertyChanged(nameof(Descripciones2Lista));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar datos\n{ex.Message}", 0);
            }
        }

        private bool FiltrarArticulos(object obj)
        {
            if (obj is not Articulo art)
                return false;

            bool coincideCodigo =
                string.IsNullOrWhiteSpace(FiltroCodigo) ||
                (art.Codigo?.Contains(FiltroCodigo, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideDescrip =
                string.IsNullOrWhiteSpace(FiltroDescripcion) ||
                (art.Descrip?.Contains(FiltroDescripcion, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideDescrip2 =
                string.IsNullOrWhiteSpace(FiltroDescripcion2) ||
                (art.Descrip2?.Contains(FiltroDescripcion2, StringComparison.OrdinalIgnoreCase) ?? false);

            return coincideCodigo && coincideDescrip && coincideDescrip2;
        }

        // -----------------------------
        // LIMPIAR FILTROS
        // -----------------------------

        public void LimpiarFiltros()
        {
            FiltroCodigo = "";
            FiltroDescripcion = "";
            FiltroDescripcion2 = "";
        }

        // -----------------------------
        // CRUD Y ALMACÉN (TU CÓDIGO)
        // -----------------------------

        public string Cantidad { get; set; }
        public string Pasillo { get; set; }
        public string Estanteria { get; set; }
        public string Hueco { get; set; }

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


        public string Codigo
        {
            get => _articulo.Codigo;
            set { _articulo.Codigo = value; OnPropertyChanged(); }
        }

        public string Descrip
        {
            get => _articulo.Descrip;
            set { _articulo.Descrip = value; OnPropertyChanged(); }
        }

        public string? Descrip2
        {
            get => _articulo.Descrip2;
            set { _articulo.Descrip2 = value; OnPropertyChanged(); }
        }

        public double? Pvp
        {
            get => _articulo.Pvp;
            set { _articulo.Pvp = value; OnPropertyChanged(); }
        }

        public decimal? PrecioCompra
        {
            get => _articulo.PrecioCompra;
            set { _articulo.PrecioCompra = value; OnPropertyChanged(); }
        }

        public int? Stock
        {
            get => _articulo.Stock;
            set { _articulo.Stock = value; OnPropertyChanged(); }
        }

        public async Task<bool> GuardarAsync()
        {
            try
            {
                await _articuloRepository.AddAsync(_articulo);
                return true;
            }
            catch
            {
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
            catch
            {
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
            catch
            {
                return false;
            }
        }

        public async Task SalidaAlmacen()
        {
            // tu código original
        }

        public async Task AñadirAlmacen()
        {
            // tu código original
        }
    }
}
