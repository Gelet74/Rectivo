using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System;
using System.Collections.Generic;
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

        public List<Cliente> listaClientes => _listaClientes;
        public List<Empleado> listaEmpleados => _listaEmpleados;
        public List<Escandallo> listaEscandallos => _listaEscandallos;
        public List<Orden> listaOrdenes => _listaOrdenes;

        public Articulo articulo
        {
            get => _articulo;
            set => SetProperty(ref _articulo, value);
        }

        private List<string> _codigosArticulos;
        public List<string> CodigosArticulos
        {
            get => _codigosArticulos;
            set => SetProperty(ref _codigosArticulos, value);
        }

        private string _codigoSeleccionado;
        public string CodigoSeleccionado
        {
            get => _codigoSeleccionado;
            set => SetProperty(ref _codigoSeleccionado, value);
        }

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

        public decimal? PrecioCompra
        {
            get => _articulo.PrecioCompra;
            set { _articulo.PrecioCompra = value; OnPropertyChanged(); }
        }

        public async Task Inicializa()
        {
            try
            {
                // 0) Cargar códigos primero
                await LoadCodigosAsync();
                await Task.Delay(10);

                // 1) ARTÍCULOS
                ListaArticulos = (List<Articulo>)await _articuloRepository.GetAllAsync();
                await Task.Delay(10);

                // 2) CLIENTES
                if (_clienteRepository != null)
                {
                    _listaClientes = (List<Cliente>)await _clienteRepository.GetAllAsync();
                    await Task.Delay(10);
                }

                // 3) EMPLEADOS
                if (_empleadoRepository != null)
                {
                    _listaEmpleados = (List<Empleado>)await _empleadoRepository.GetAllAsync();
                    await Task.Delay(10);
                }

                // 4) ESCANDALLOS
                if (_escandalloRepository != null)
                {
                    _listaEscandallos = (List<Escandallo>)await _escandalloRepository.GetAllAsync();
                    await Task.Delay(10);
                }

                // 5) ORDENES
                if (_ordenRepository != null)
                {
                    _listaOrdenes = (List<Orden>)await _ordenRepository.GetAllAsync();
                    await Task.Delay(10);
                }
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
                await _articuloRepository.UpdateAsync(articulo);
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al modificar artículo: {ex.Message}");
                return false;
            }
        }
    }
}
