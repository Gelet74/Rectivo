using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace recTivo.MVVM
{
    public class MVArticulo : MVBase
    {
        private Articulo _articulo;

        private readonly ArticuloRepository _articuloRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly EscandalloRepository _escandalloRepository;
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly OrdenRepository _ordenRepository;

        private List<Articulo> _listaArticulos;
        private List<Cliente> _listaClientes;
        private List<Empleado> _listaEmpleados;
        private List<Escandallo> _listaEscandallos;
        private List<Orden> _listaOrdenes;

        public List<Articulo> ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }

        // ============================================================
        //   SELECCIÓN DE PADRE PARA AÑADIR SUBCOMPONENTES
        // ============================================================
        private ComponenteEscandallo _componentePadreSeleccionado;
        public ComponenteEscandallo ComponentePadreSeleccionado
        {
            get => _componentePadreSeleccionado;
            set => SetProperty(ref _componentePadreSeleccionado, value);
        }

        public List<string> CodigosArticulos
        {
            get => _codigosArticulos;
            set => SetProperty(ref _codigosArticulos, value);
        }
        private List<string> _codigosArticulos;

        public string CodigoSeleccionado
        {
            get => _codigoSeleccionado;
            set => SetProperty(ref _codigoSeleccionado, value);
        }
        private string _codigoSeleccionado;

        // ============================================================
        //   ARTÍCULO FINAL (ALTA ESCANDALLO)
        // ============================================================
        private Articulo _articuloFinal;
        public Articulo ArticuloFinal
        {
            get => _articuloFinal;
            set
            {
                _articuloFinal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));
            }
        }

        public string DescripcionFinal => ArticuloFinal?.Descrip ?? "";
        public string Descripcion2Final => ArticuloFinal?.Descrip2 ?? "";

        // ============================================================
        //   COMPONENTES PARA ALTA ESCANDALLO
        // ============================================================
        public ObservableCollection<ComponenteEscandallo> Componentes { get; set; } = new();
        public ComponenteEscandallo ComponenteNuevo { get; set; } = new() { Cantidad = 1 };

        // ------------------------------------------------------------
        //   AÑADIR COMPONENTE RAÍZ
        // ------------------------------------------------------------
        public void AñadirComponente()
        {
            if (ArticuloFinal == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo PT como raíz.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(ComponenteNuevo.CodigoArticulo) &&
                ComponenteNuevo.Cantidad > 0)
            {
                if (ComponenteNuevo.CodigoArticulo.StartsWith("PT"))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Un artículo PT no puede ser componente.");
                    return;
                }

                var articulo = ListaArticulos.FirstOrDefault(a => a.Codigo == ComponenteNuevo.CodigoArticulo);

                Componentes.Add(new ComponenteEscandallo
                {
                    CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                    Cantidad = ComponenteNuevo.Cantidad,
                    Descripcion = articulo?.Descrip ?? "",
                    Descripcion2 = articulo?.Descrip2 ?? "",
                    Hijos = new List<ComponenteEscandallo>()
                });

                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                OnPropertyChanged(nameof(ComponenteNuevo));
            }
        }

        // ------------------------------------------------------------
        //   AÑADIR SUBCOMPONENTE
        // ------------------------------------------------------------
        public void AñadirSubcomponente()
        {
            if (ComponentePadreSeleccionado == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un componente padre.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ComponenteNuevo.CodigoArticulo) ||
                ComponenteNuevo.Cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO", "Código o cantidad inválidos.");
                return;
            }

            if (ComponenteNuevo.CodigoArticulo.StartsWith("PT"))
            {
                MensajeError.Mostrar("ESCANDALLO", "Un artículo PT no puede ser hijo.");
                return;
            }

            var articulo = ListaArticulos.FirstOrDefault(a => a.Codigo == ComponenteNuevo.CodigoArticulo);

            var nuevo = new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.Descrip ?? "",
                Descripcion2 = articulo?.Descrip2 ?? "",
                CodigoComponentePadre = ComponentePadreSeleccionado.CodigoArticulo,
                Hijos = new List<ComponenteEscandallo>()
            };

            ComponentePadreSeleccionado.Hijos.Add(nuevo);

            OnPropertyChanged(nameof(Componentes));

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            OnPropertyChanged(nameof(ComponenteNuevo));
        }

        // ------------------------------------------------------------
        //   GUARDAR ESCANDALLO COMPLETO (RECURSIVO)
        // ------------------------------------------------------------
        public async Task GuardarEscandallo()
        {
            try
            {
                if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo final.");
                    return;
                }

                if (!ArticuloFinal.Codigo.StartsWith("PT"))
                {
                    MensajeError.Mostrar("ESCANDALLO", "El artículo raíz debe empezar por PT.");
                    return;
                }

                var articuloFinal = await _articuloRepository.GetByCodigoAsync(ArticuloFinal.Codigo);
                if (articuloFinal == null)
                {
                    MensajeError.Mostrar("ESCANDALLO", "El código del artículo final no existe.");
                    return;
                }

                var nuevoEscandallo = new Escandallo
                {
                    CodigoProducto = articuloFinal.Codigo,
                    NombreProducto = articuloFinal.Descrip,
                    Descripcion2 = articuloFinal.Descrip2
                };

                await _escandalloRepository.AddAsync(nuevoEscandallo);

                foreach (var comp in Componentes)
                    await GuardarComponenteRecursivo(comp, nuevoEscandallo.IdEscandallo, null);

                MensajeInformacion.Mostrar("ESCANDALLO", "Escandallo guardado correctamente.", 1);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al guardar el escandallo:\n{ex.Message}");
            }

            ArticuloFinal = null;
            Componentes.Clear();
            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };

            OnPropertyChanged(nameof(ArticuloFinal));
            OnPropertyChanged(nameof(DescripcionFinal));
            OnPropertyChanged(nameof(Descripcion2Final));
            OnPropertyChanged(nameof(ComponenteNuevo));
        }

        private async Task GuardarComponenteRecursivo(
            ComponenteEscandallo comp,
            int idEscandallo,
            string codigoPadre)
        {
            var articuloComp = await _articuloRepository.GetByCodigoAsync(comp.CodigoArticulo);
            if (articuloComp == null)
                return;

            var nuevoComp = new ComponenteEscandallo
            {
                IdEscandallo = idEscandallo,
                CodigoArticulo = articuloComp.Codigo,
                Descripcion = articuloComp.Descrip,
                Descripcion2 = articuloComp.Descrip2,
                Cantidad = comp.Cantidad,
                PrecioUnitario = articuloComp.PrecioCompra ?? 0,
                CodigoComponentePadre = codigoPadre
            };

            await _escandalloRepository.InsertComponenteAsync(nuevoComp);

            foreach (var hijo in comp.Hijos)
                await GuardarComponenteRecursivo(hijo, idEscandallo, nuevoComp.CodigoArticulo);
        }

        // ============================================================
        //   MÉTODOS DE ARTÍCULOS (ALTA, BAJA, MODIFICAR, CARGAR)
        // ============================================================

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

        // ============================================================
        //   CONSTRUCTOR
        // ============================================================
        public MVArticulo(
               ArticuloRepository articuloRepository,
               ClienteRepository clienteRepository,
               EscandalloRepository escandalloRepository,
               EmpleadoRepository empleadoRepository,
               OrdenRepository ordenRepository)
        {
            _articuloRepository = articuloRepository;
            _clienteRepository = clienteRepository;
            _escandalloRepository = escandalloRepository;
            _empleadoRepository = empleadoRepository;
            _ordenRepository = ordenRepository;

            _articulo = new Articulo();
        }

        // ============================================================
        //   CAMPOS DEL ARTÍCULO
        // ============================================================
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

        // ============================================================
        //   INICIALIZACIÓN
        // ============================================================
        public async Task Inicializa()
        {
            try
            {
                await LoadCodigosAsync();
                await Task.Delay(10);

                ListaArticulos = (List<Articulo>)await _articuloRepository.GetAllAsync();
                await Task.Delay(10);

                if (_clienteRepository != null)
                    _listaClientes = (List<Cliente>)await _clienteRepository.GetAllAsync();

                if (_empleadoRepository != null)
                    _listaEmpleados = (List<Empleado>)await _empleadoRepository.GetAllAsync();

                if (_escandalloRepository != null)
                    _listaEscandallos = (List<Escandallo>)await _escandalloRepository.GetAllAsync();

                if (_ordenRepository != null)
                    _listaOrdenes = (List<Orden>)await _ordenRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("GESTIÓN ARTÍCULOS", $"Error al cargar datos\n{ex.Message}", 0);
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

        // ============================================================
        //   LISTAR ESCANDALLO (TreeView)
        // ============================================================
        private ObservableCollection<ComponenteEscandallo> _escandalloActual
            = new ObservableCollection<ComponenteEscandallo>();
        public ObservableCollection<ComponenteEscandallo> EscandalloActual
        {
            get => _escandalloActual;
            set => SetProperty(ref _escandalloActual, value);
        }

        private ComponenteEscandallo _componenteSeleccionado;
        public ComponenteEscandallo ComponenteSeleccionado
        {
            get => _componenteSeleccionado;
            set => SetProperty(ref _componenteSeleccionado, value);
        }

        private string _descripcionArticulo;
        public string DescripcionArticulo
        {
            get => _descripcionArticulo;
            set => SetProperty(ref _descripcionArticulo, value);
        }

        public async Task CargarEscandalloAsync(string codigo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes introducir un código.");
                    return;
                }

                if (!codigo.StartsWith("PT"))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Solo se pueden cargar escandallos de artículos PT.");
                    return;
                }

                var esc = await _escandalloRepository
                    .Query()
                    .FirstOrDefaultAsync(e => e.CodigoProducto == codigo);

                if (esc == null)
                {
                    MensajeError.Mostrar("ESCANDALLO", $"No existe escandallo para '{codigo}'.");
                    return;
                }

                // Cargar artículo raíz
                ArticuloFinal = await _articuloRepository.GetByCodigoAsync(esc.CodigoProducto);
                OnPropertyChanged(nameof(ArticuloFinal));
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));

                DescripcionArticulo = $"{ArticuloFinal.Descrip} - {ArticuloFinal.Descrip2}";

                var componentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(esc.IdEscandallo);

                var raiz = componentes
                    .Where(c => c.CodigoComponentePadre == null)
                    .ToList();

                foreach (var comp in raiz)
                    ConstruirHijos(comp, componentes);

                EscandalloActual = new ObservableCollection<ComponenteEscandallo>(raiz);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        private void ConstruirHijos(ComponenteEscandallo padre, List<ComponenteEscandallo> todos)
        {
            padre.Hijos = todos
                .Where(c => c.CodigoComponentePadre == padre.CodigoArticulo)
                .ToList();

            foreach (var hijo in padre.Hijos)
                ConstruirHijos(hijo, todos);
        }
    }
}
