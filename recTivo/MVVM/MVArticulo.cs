using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

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

        private List<Articulo> _listaArticulos;
        private List<Cliente> _listaClientes;
        private List<Empleado> _listaEmpleados;
        private List<Orden> _listaOrdenes;

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

        public List<Articulo> ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }

        public List<string> CodigosArticulos
        {
            get => _codigosArticulos;
            set => SetProperty(ref _codigosArticulos, value);
        }
        private List<string> _codigosArticulos;

        private string _codigoSeleccionado;
        public string CodigoSeleccionado
        {
            get => _codigoSeleccionado;
            set
            {
                if (SetProperty(ref _codigoSeleccionado, value))
                {
                    FiltrarPorCodigo();
                }
            }
        }

        public decimal? PrecioCompra
        {
            get => _articulo.PrecioCompra;
            set { _articulo.PrecioCompra = value; OnPropertyChanged(); }
        }

        public string Codigo
        {
            get => _articulo.Codigo;
            set { _articulo.Codigo = value; OnPropertyChanged(); }
        }

        private int _totalArticulos;
        public int TotalArticulos
        {
            get => _totalArticulos;
            set => SetProperty(ref _totalArticulos, value);
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

        public int? Stock
        {
            get => _articulo.Stock;
            set { _articulo.Stock = value; OnPropertyChanged(); }
        }

        public async Task Inicializa()
        {
            try
            {
                await LoadCodigosAsync();

                ListaArticulos = (List<Articulo>)await _articuloRepository.GetAllAsync();

                if (_clienteRepository != null)
                    _listaClientes = (List<Cliente>)await _clienteRepository.GetAllAsync();

                if (_empleadoRepository != null)
                    _listaEmpleados = (List<Empleado>)await _empleadoRepository.GetAllAsync();

                if (_ordenRepository != null)
                    _listaOrdenes = (List<Orden>)await _ordenRepository.GetAllAsync();

                CodigosFiltrados = ListaArticulos.Select(a => a.Codigo).Distinct().ToList();
                Descrip1Filtradas = ListaArticulos.Select(a => a.Descrip).Distinct().ToList();
                Descrip2Filtradas = ListaArticulos.Select(a => a.Descrip2).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();

                ArticulosFiltrados.Clear();
                foreach (var a in ListaArticulos)
                    ArticulosFiltrados.Add(a);

                Descrip1FiltradasOC.Clear();
                foreach (var d in ListaArticulos.Select(a => a.Descrip).Distinct())
                    Descrip1FiltradasOC.Add(d);

                Descrip2FiltradasOC.Clear();
                foreach (var d2 in ListaArticulos.Select(a => a.Descrip2).Where(d => !string.IsNullOrEmpty(d)).Distinct())
                    Descrip2FiltradasOC.Add(d2);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar datos\n{ex.Message}", 0);
            }
        }

        private async Task LoadCodigosAsync()
        {
            try
            {
                CodigosArticulos = await _articuloRepository.Query(true)
                                                            .Select(a => a.Codigo)
                                                            .ToListAsync();
            }
            catch
            {
                CodigosArticulos = new List<string>();
            }
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

        public async Task<bool> BajaPorCodigoAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CodigoSeleccionado))
                    return false;

                var articulo = await _articuloRepository.GetByCodigoAsync(CodigoSeleccionado);
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

        public async Task<bool> CargarArticuloSeleccionadoAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CodigoSeleccionado))
                    return false;

                string codigo = CodigoSeleccionado.Trim().ToUpper();

                var art = await _articuloRepository.GetByCodigoAsync(codigo);

                if (art == null)
                {
                    MensajeError.Mostrar("DEBUG", $"GetByCodigoAsync devolvió null para '{codigo}'");
                    return false;
                }

                _articulo = art;

                OnPropertyChanged(nameof(Codigo));
                OnPropertyChanged(nameof(Descrip));
                OnPropertyChanged(nameof(Descrip2));
                OnPropertyChanged(nameof(Pvp));
                OnPropertyChanged(nameof(Stock));
                OnPropertyChanged(nameof(PrecioCompra));

                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar artículo: {ex.Message}");
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

        private List<string> _codigosFiltrados;
        public List<string> CodigosFiltrados
        {
            get => _codigosFiltrados;
            set => SetProperty(ref _codigosFiltrados, value);
        }

        private List<string> _descrip1Filtradas;
        public List<string> Descrip1Filtradas
        {
            get => _descrip1Filtradas;
            set => SetProperty(ref _descrip1Filtradas, value);
        }

        private List<string> _descrip2Filtradas;
        public List<string> Descrip2Filtradas
        {
            get => _descrip2Filtradas;
            set => SetProperty(ref _descrip2Filtradas, value);
        }

        private string _descrip1Seleccionada;
        public string Descrip1Seleccionada
        {
            get => _descrip1Seleccionada;
            set
            {
                if (SetProperty(ref _descrip1Seleccionada, value))
                {
                    FiltrarPorDescrip1();
                }
            }
        }

        private string _descrip2Seleccionada;
        public string Descrip2Seleccionada
        {
            get => _descrip2Seleccionada;
            set
            {
                if (SetProperty(ref _descrip2Seleccionada, value))
                {
                    FiltrarPorDescrip2();
                }
            }
        }

        private void FiltrarPorCodigo()
        {
            if (ListaArticulos == null) return;

            if (string.IsNullOrWhiteSpace(CodigoSeleccionado))
            {
                RestablecerFiltros();
                return;
            }

            var articulosFiltrados = ListaArticulos
                .Where(a => a.Codigo == CodigoSeleccionado)
                .OrderBy(a => a.Codigo)
                .ToList();

            if (articulosFiltrados.Any())
            {
                // Autocompletar si solo hay un artículo con ese código
                if (articulosFiltrados.Count == 1)
                {
                    var articulo = articulosFiltrados.First();
                    _descrip1Seleccionada = articulo.Descrip;
                    _descrip2Seleccionada = articulo.Descrip2;
                    OnPropertyChanged(nameof(Descrip1Seleccionada));
                    OnPropertyChanged(nameof(Descrip2Seleccionada));

                    // También establecer el artículo seleccionado
                    ArticuloSeleccionado = articulo;
                }

                // Actualizar filtros
                ActualizarFiltros(articulosFiltrados);
            }
        }

        private void FiltrarPorDescrip1()
        {
            if (ListaArticulos == null) return;

            var query = ListaArticulos.AsQueryable();

            // Aplicar filtros acumulativos
            if (!string.IsNullOrWhiteSpace(Descrip1Seleccionada))
                query = query.Where(a => a.Descrip == Descrip1Seleccionada);

            if (!string.IsNullOrWhiteSpace(Descrip2Seleccionada))
                query = query.Where(a => a.Descrip2 == Descrip2Seleccionada);

            if (!string.IsNullOrWhiteSpace(CodigoSeleccionado))
                query = query.Where(a => a.Codigo == CodigoSeleccionado);

            var articulosFiltrados = query.ToList();

            // Si solo queda un artículo, autocompletar
            if (articulosFiltrados.Count == 1)
            {
                var articulo = articulosFiltrados.First();
                _codigoSeleccionado = articulo.Codigo;
                _descrip2Seleccionada = articulo.Descrip2;
                OnPropertyChanged(nameof(CodigoSeleccionado));
                OnPropertyChanged(nameof(Descrip2Seleccionada));

                ArticuloSeleccionado = articulo;
            }

            ActualizarFiltros(articulosFiltrados);
        }

        private void FiltrarPorDescrip2()
        {
            if (ListaArticulos == null) return;

            var query = ListaArticulos.AsQueryable();

            // Aplicar filtros acumulativos
            if (!string.IsNullOrWhiteSpace(Descrip2Seleccionada))
                query = query.Where(a => a.Descrip2 == Descrip2Seleccionada);

            if (!string.IsNullOrWhiteSpace(Descrip1Seleccionada))
                query = query.Where(a => a.Descrip == Descrip1Seleccionada);

            if (!string.IsNullOrWhiteSpace(CodigoSeleccionado))
                query = query.Where(a => a.Codigo == CodigoSeleccionado);

            var articulosFiltrados = query.ToList();

            // Si solo queda un artículo, autocompletar
            if (articulosFiltrados.Count == 1)
            {
                var articulo = articulosFiltrados.First();
                _codigoSeleccionado = articulo.Codigo;
                _descrip1Seleccionada = articulo.Descrip;
                OnPropertyChanged(nameof(CodigoSeleccionado));
                OnPropertyChanged(nameof(Descrip1Seleccionada));

                ArticuloSeleccionado = articulo;
            }

            ActualizarFiltros(articulosFiltrados);
        }

        private void ActualizarFiltros(List<Articulo> articulosFiltrados)
        {
            // Actualizar listas filtradas
            CodigosFiltrados = articulosFiltrados.Select(a => a.Codigo).Distinct().ToList();
            Descrip1Filtradas = articulosFiltrados.Select(a => a.Descrip).Distinct().ToList();
            Descrip2Filtradas = articulosFiltrados.Select(a => a.Descrip2)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();

            // Actualizar ObservableCollections para la UI
            ArticulosFiltrados.Clear();
            foreach (var a in articulosFiltrados)
                ArticulosFiltrados.Add(a);

            Descrip1FiltradasOC.Clear();
            foreach (var d in Descrip1Filtradas)
                Descrip1FiltradasOC.Add(d);

            Descrip2FiltradasOC.Clear();
            foreach (var d2 in Descrip2Filtradas)
                Descrip2FiltradasOC.Add(d2);
        }

        private void RestablecerFiltros()
        {
            if (ListaArticulos == null) return;

            CodigosFiltrados = ListaArticulos.Select(a => a.Codigo).Distinct().ToList();
            Descrip1Filtradas = ListaArticulos.Select(a => a.Descrip).Distinct().ToList();
            Descrip2Filtradas = ListaArticulos.Select(a => a.Descrip2)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();

            ArticulosFiltrados.Clear();
            foreach (var a in ListaArticulos)
                ArticulosFiltrados.Add(a);

            Descrip1FiltradasOC.Clear();
            foreach (var d in Descrip1Filtradas)
                Descrip1FiltradasOC.Add(d);

            Descrip2FiltradasOC.Clear();
            foreach (var d2 in Descrip2Filtradas)
                Descrip2FiltradasOC.Add(d2);
        }

        public ObservableCollection<Articulo> ArticulosFiltrados { get; set; } = new();
        public ObservableCollection<string> Descrip1FiltradasOC { get; set; } = new();
        public ObservableCollection<string> Descrip2FiltradasOC { get; set; } = new();

        private Articulo _articuloSeleccionado;
        public Articulo ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set
            {
                SetProperty(ref _articuloSeleccionado, value);
                if (value != null)
                {
                    Descrip1Seleccionada = value.Descrip;
                    Descrip2Seleccionada = value.Descrip2;
                }
            }
        }

        public string Cantidad { get; set; }
        public string Pasillo { get; set; }
        public string Estanteria { get; set; }
        public string Hueco { get; set; }

        public async Task SalidaAlmacen()
        {
            try
            {
                if (ArticuloSeleccionado == null)
                {
                    MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo válido.");
                    return;
                }

                if (!int.TryParse(Cantidad, out int cantidad) || cantidad <= 0)
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Introduce una cantidad válida.");
                    return;
                }

                // Verificar que hay stock suficiente
                if ((ArticuloSeleccionado.Stock ?? 0) < cantidad)
                {
                    MensajeError.Mostrar("ERROR",
                        $"Stock insuficiente. Disponible: {ArticuloSeleccionado.Stock ?? 0}, Solicitado: {cantidad}");
                    return;
                }

                string pasillo = Pasillo?.Trim();
                string estanteria = Estanteria?.Trim();
                string hueco = Hueco?.Trim();

                if (string.IsNullOrEmpty(pasillo) ||
                    string.IsNullOrEmpty(estanteria) ||
                    string.IsNullOrEmpty(hueco))
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Debes indicar pasillo, estantería y hueco de donde se retira.");
                    return;
                }

                int? estanteriaNum = int.TryParse(estanteria, out var est) ? est : null;
                int? huecoNum = int.TryParse(hueco, out var hue) ? hue : null;

                var ctx = _context;

                // Verificar que la ubicación existe y coincide con la del artículo
                var ubicacion = await ctx.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.LetraPasillo == pasillo &&
                        u.NumeroEstanteria == estanteriaNum &&
                        u.Numero == huecoNum);

                if (ubicacion == null || ArticuloSeleccionado.IdUbicacion != ubicacion.IdUbicacion)
                {
                    MensajeError.Mostrar("ERROR", "La ubicación especificada no coincide con la del artículo.");
                    return;
                }

                // Restar del stock
                ArticuloSeleccionado.Stock = (ArticuloSeleccionado.Stock ?? 0) - cantidad;

                // Si el stock llega a 0, quitar la ubicación
                if (ArticuloSeleccionado.Stock == 0)
                {
                    ArticuloSeleccionado.IdUbicacion = null;
                }

                ctx.Articulos.Update(ArticuloSeleccionado);
                await ctx.SaveChangesAsync();

                MensajeInformacion.Mostrar("ÉXITO",
                    $"Se retiraron {cantidad} unidades del artículo {ArticuloSeleccionado.Codigo} " +
                    $"del pasillo {pasillo}, estantería {estanteria}, hueco {hueco}. " +
                    $"Stock restante: {ArticuloSeleccionado.Stock}");

                // Limpiar campos
                Cantidad = "";
                Pasillo = "";
                Estanteria = "";
                Hueco = "";
                ArticuloSeleccionado = null;
                Descrip1Seleccionada = null;
                Descrip2Seleccionada = null;

                OnPropertyChanged(nameof(Cantidad));
                OnPropertyChanged(nameof(Pasillo));
                OnPropertyChanged(nameof(Estanteria));
                OnPropertyChanged(nameof(Hueco));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al retirar del almacén: {ex.Message}");
            }
        }

        public async Task AñadirAlmacen()
        {
            try
            {
                if (ArticuloSeleccionado == null)
                {
                    MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo válido.");
                    return;
                }

                if (!int.TryParse(Cantidad, out int cantidad) || cantidad <= 0)
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Introduce una cantidad válida.");
                    return;
                }

                string pasillo = Pasillo?.Trim();
                string estanteria = Estanteria?.Trim();
                string hueco = Hueco?.Trim();

                if (string.IsNullOrEmpty(pasillo) ||
                    string.IsNullOrEmpty(estanteria) ||
                    string.IsNullOrEmpty(hueco))
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Debes indicar pasillo, estantería y hueco.");
                    return;
                }

                int? estanteriaNum = int.TryParse(estanteria, out var est) ? est : null;
                int? huecoNum = int.TryParse(hueco, out var hue) ? hue : null;

                var ctx = _context;

                var ubicacion = await ctx.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.LetraPasillo == pasillo &&
                        u.NumeroEstanteria == estanteriaNum &&
                        u.Numero == huecoNum);

                if (ubicacion == null)
                {
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = pasillo,
                        NumeroEstanteria = estanteriaNum,
                        Numero = huecoNum
                    };
                    ctx.Ubicacion.Add(ubicacion);
                    await ctx.SaveChangesAsync();
                }

                ArticuloSeleccionado.Stock = (ArticuloSeleccionado.Stock ?? 0) + cantidad;
                ArticuloSeleccionado.IdUbicacion = ubicacion.IdUbicacion;

                ctx.Articulos.Update(ArticuloSeleccionado);
                await ctx.SaveChangesAsync();

                MensajeInformacion.Mostrar("ÉXITO",
                    $"Se añadieron {cantidad} unidades del artículo {ArticuloSeleccionado.Codigo} " +
                    $"al pasillo {pasillo}, estantería {estanteria}, hueco {hueco}.");

                Cantidad = "";
                Pasillo = "";
                Estanteria = "";
                Hueco = "";
                ArticuloSeleccionado = null;
                Descrip1Seleccionada = null;
                Descrip2Seleccionada = null;

                OnPropertyChanged(nameof(Cantidad));
                OnPropertyChanged(nameof(Pasillo));
                OnPropertyChanged(nameof(Estanteria));
                OnPropertyChanged(nameof(Hueco));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al añadir al almacén: {ex.Message}");
            }
        }
    }
}