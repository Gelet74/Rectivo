using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;

namespace recTivo.MVVM
{
    public class MVCliente : MVBase
    {
        private Cliente _cliente;

        private readonly ClienteRepository _clienteRepository;
        private readonly RectivoContext _context;
        private readonly ArticuloRepository _articuloRepository;

        public ListCollectionView ClientesView { get; private set; }

        public MVCliente(
            ClienteRepository clienteRepository,
            RectivoContext context,
            ArticuloRepository articuloRepository)
        {
            _clienteRepository = clienteRepository;
            _context = context;
            _articuloRepository = articuloRepository;
        }

        // ============================================================
        // LISTADO
        // ============================================================

        private ObservableCollection<Cliente> _listaClientes;
        public ObservableCollection<Cliente> ListaClientes
        {
            get => _listaClientes;
            set => SetProperty(ref _listaClientes, value);
        }

        // ============================================================
        // SELECCIÓN
        // ============================================================

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set => SetProperty(ref _clienteSeleccionado, value);
        }

        // ============================================================
        // CAMPOS DEL FORMULARIO (con [Required] para validación visual)
        // ============================================================

        private string _nombre;
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        public string Nombre
        {
            get => _nombre;
            set
            {
                SetProperty(ref _nombre, value);
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        private string _apellido1;
        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        public string Apellido1
        {
            get => _apellido1;
            set
            {
                SetProperty(ref _apellido1, value);
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        private string _apellido2;
        public string Apellido2
        {
            get => _apellido2;
            set => SetProperty(ref _apellido2, value);
        }

        private string _dni;
        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni
        {
            get => _dni;
            set
            {
                SetProperty(ref _dni, value);
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _filtroNombre;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set
            {
                SetProperty(ref _filtroNombre, value);
                ClientesView?.Refresh();
            }
        }

        private string _filtroApellido1;
        public string FiltroApellido1
        {
            get => _filtroApellido1;
            set
            {
                SetProperty(ref _filtroApellido1, value);
                ClientesView?.Refresh();
            }
        }

        private string _filtroApellido2;
        public string FiltroApellido2
        {
            get => _filtroApellido2;
            set
            {
                SetProperty(ref _filtroApellido2, value);
                ClientesView?.Refresh();
            }
        }

        private string _DNI;
        public string DNI
        {
            get => _DNI;
            set
            {
                SetProperty(ref _DNI, value);
                ClientesView?.Refresh();
            }
        }

        private string _usuario;
        [Required(ErrorMessage = "El Usuario es obligatorio")]
        public string Usuario
        {
            get => _usuario;
            set
            {
                SetProperty(ref _usuario, value);
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        private string _password;
        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        public List<string> NombreLista => ListaClientes?
            .Select(a => a.Nombre)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        public List<string> Apellido1Lista => ListaClientes?
           .Select(a => a.Apellido1)
           .Where(d => !string.IsNullOrWhiteSpace(d))
           .Distinct()
           .OrderBy(d => d)
           .ToList();

        public List<string> Apellido2Lista => ListaClientes?
          .Select(a => a.Apellido2)
          .Where(d => !string.IsNullOrWhiteSpace(d))
          .Distinct()
          .OrderBy(d => d)
          .ToList();

        // ============================================================
        // VALIDACIÓN BOTÓN ALTA
        // ============================================================

        public override bool HasNoErrors =>
            !HasErrors &&
            !string.IsNullOrWhiteSpace(Nombre) &&
            !string.IsNullOrWhiteSpace(Apellido1) &&
            !string.IsNullOrWhiteSpace(Dni) &&
            !string.IsNullOrWhiteSpace(Usuario) &&
            !string.IsNullOrWhiteSpace(Password);

        // ============================================================
        // INICIALIZAR
        // ============================================================

        public async Task Inicializa()
        {
            try
            {
                ListaClientes = new ObservableCollection<Cliente>(
                    (await _clienteRepository.GetAllAsync())
                    .OrderBy(c => c.Apellido1)
                );

                ClientesView = new ListCollectionView(ListaClientes);
                ClientesView.Filter = FiltarClientes;

                OnPropertyChanged(nameof(ClientesView));
                OnPropertyChanged(nameof(NombreLista));
                OnPropertyChanged(nameof(Apellido1Lista));
                OnPropertyChanged(nameof(Apellido2Lista));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar datos\n{ex.Message}", 0);
            }
        }

        public void LimpiarFiltros()
        {
            FiltroNombre = "";
            FiltroApellido1 = "";
            FiltroApellido2 = "";
        }

        private bool FiltarClientes(object obj)
        {
            if (obj is not Cliente cliente)
                return false;

            bool coincideNombre =
                string.IsNullOrWhiteSpace(FiltroNombre) ||
                (cliente.Nombre?.Contains(FiltroNombre, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideApellido1 =
                string.IsNullOrWhiteSpace(FiltroApellido1) ||
                (cliente.Apellido1?.Contains(FiltroApellido1, StringComparison.OrdinalIgnoreCase) ?? false);

            bool coincideApellido2 =
                string.IsNullOrWhiteSpace(FiltroApellido2) ||
                (cliente.Apellido2?.Contains(FiltroApellido2, StringComparison.OrdinalIgnoreCase) ?? false);

            return coincideNombre && coincideApellido1 && coincideApellido2;
        }

        // ============================================================
        // ALTA
        // ============================================================

        public async Task<bool> GuardarAsync()
        {
            var cliente = new Cliente
            {
                Nombre = Nombre,
                Apellido1 = Apellido1,
                Apellido2 = Apellido2,
                Dni = Dni,
                Telefono = Telefono,
                Usuario = Usuario,
                Password = BCrypt.Net.BCrypt.HashPassword(Password)
            };

            await _clienteRepository.AddAsync(cliente);
            ListaClientes.Add(cliente);
            ClientesView.Refresh();
            LimpiarCampos();
            return true;
        }

        // ============================================================
        // CARGAR CLIENTE SELECCIONADO
        // ============================================================

        public async Task<bool> CargarClienteSeleccionadoAsync()
        {
            if (ClienteSeleccionado == null)
                return false;

            Nombre = ClienteSeleccionado.Nombre;
            Apellido1 = ClienteSeleccionado.Apellido1;
            Apellido2 = ClienteSeleccionado.Apellido2;
            Dni = ClienteSeleccionado.Dni;
            Telefono = ClienteSeleccionado.Telefono;
            Usuario = ClienteSeleccionado.Usuario;
            Password = ClienteSeleccionado.Password;

            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellido1));
            OnPropertyChanged(nameof(Apellido2));
            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Telefono));
            OnPropertyChanged(nameof(Usuario));
            OnPropertyChanged(nameof(Password));

            return true;
        }

        // ============================================================
        // MODIFICAR
        // ============================================================

        public async Task<bool> ModificarClienteAsync()
        {
            if (ClienteSeleccionado == null)
                return false;

            ClienteSeleccionado.Nombre = Nombre;
            ClienteSeleccionado.Apellido1 = Apellido1;
            ClienteSeleccionado.Apellido2 = Apellido2;
            ClienteSeleccionado.Dni = Dni;
            ClienteSeleccionado.Telefono = Telefono;
            ClienteSeleccionado.Usuario = Usuario;

            if (Password != ClienteSeleccionado.Password)
            {
                ClienteSeleccionado.Password = BCrypt.Net.BCrypt.HashPassword(Password);
            }

            await _clienteRepository.UpdateAsync(ClienteSeleccionado);
            await Inicializa();

            return true;
        }

        // ============================================================
        // LIMPIAR CAMPOS
        // ============================================================

        public void LimpiarCampos()
        {
            Nombre = null;
            Apellido1 = null;
            Apellido2 = null;
            Dni = null;
            Telefono = null;
            Usuario = null;
            Password = null;
            ClienteSeleccionado = null;

            OnPropertyChanged(nameof(HasNoErrors));
        }

        // ============================================================
        // TOTAL CLIENTES
        // ============================================================

        private int _totalClientes;
        public int TotalClientes
        {
            get => _totalClientes;
            set => SetProperty(ref _totalClientes, value);
        }

        // ============================================================
        // LISTAS FILTRADAS PARA COMBOBOX
        // ============================================================

        private List<string> _listaNombresFiltrados;
        public List<string> ListaNombresFiltrados
        {
            get => _listaNombresFiltrados;
            set => SetProperty(ref _listaNombresFiltrados, value);
        }
    }
}