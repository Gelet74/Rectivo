using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using di.proyecto.clase._2025.Frontend.Mensajes;
using System.Threading.Tasks;

namespace recTivo.MVVM
{
    public class MVArticulo : MVBase
    {        
        private Articulo _articulo;

        private ArticuloRepository _articuloRepository;
        private ClienteRepository _clienteRepository;
        private EscandalloRepository _escandalloRepository;
        private EmpleadoRepository _empleadoRepository;
        private OrdenRepository _ordenRepository;

        private List <Articulo> _listaArticulos;
        private List <Cliente> _listaClientes;
        private List <Empleado> _listaEmpleados;    
        private List <Escandallo> _listaEscandallos;
        private List <Orden> _listaOrdenes;

        public List <Articulo> listaArticulos => _listaArticulos;
        public List <Cliente> listaClientes => _listaClientes;
        public List <Empleado> listaEmpleados => _listaEmpleados;
        public List <Escandallo> listaEscandallos => _listaEscandallos;
        public List <Orden> listaOrdenes => _listaOrdenes;

        public Articulo articulo
        {
            get => _articulo;
            set => SetProperty(ref _articulo, value);
        }

        public MVArticulo(ArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
            _articulo = new Articulo();
        }
        public async Task Inicializa()
        {
            try
            {
                _listaArticulos = (List<Articulo>)await _articuloRepository.GetAllAsync();
                _listaClientes = (List<Cliente>)await _clienteRepository.GetAllAsync();
                _listaEmpleados = (List<Empleado>)await _empleadoRepository.GetAllAsync();
                _listaEscandallos = (List<Escandallo>)await _escandalloRepository.GetAllAsync();
                _listaOrdenes = (List<Orden>)await _ordenRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("GESTIÓN ARTÍCULOS", "Error al cargar datos\nNo puedo conectar con la base de datos", 0);
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

        public async Task<bool> BajaPorCodigoAsync(string codigo)
        {
            try
            {
                var articulo = await _articuloRepository.GetByCodigoAsync(codigo);
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
                MensajeError.Mostrar("Error","Error al dar de baja artículo: {ex.Message}");
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
                MensajeError.Mostrar("Error", "Error al guardar artículo: {ex.Message}");
                return false;
            }
        }
    }
}
