using recTivo.Backend.Repos;
using recTivo.MVVM.Base;

namespace recTivo.MVVM
{
    public class MVCliente : MVBase
    {
        private readonly ClienteRepository _clienteRepository;

        public MVCliente(ClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        // ============================================================
        // LISTADO
        // ============================================================

        private List<Cliente> _listaClientes;
        public List<Cliente> ListaClientes
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
        // CAMPOS DEL FORMULARIO
        // ============================================================

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _apellido1;
        public string Apellido1
        {
            get => _apellido1;
            set => SetProperty(ref _apellido1, value);
        }

        private string _apellido2;
        public string Apellido2
        {
            get => _apellido2;
            set => SetProperty(ref _apellido2, value);
        }

        private string _dni;
        public string Dni
        {
            get => _dni;
            set => SetProperty(ref _dni, value);
        }

        private string _telefono;
        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // ============================================================
        // INICIALIZAR
        // ============================================================

        public async Task Inicializa()
        {
            ListaClientes = (List<Cliente>)await _clienteRepository.GetAllAsync();
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
                Password = Password
            };

            await _clienteRepository.AddAsync(cliente);
            await Inicializa();
            LimpiarCampos();
            return true;
        }

        // ============================================================
        // BAJA
        // ============================================================

        public async Task<bool> EliminarAsync(int idCliente)
        {
            try
            {
                await _clienteRepository.DeleteAsync(idCliente);
                await Inicializa();
                return true;
            }
            catch
            {
                return false;
            }
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
            ClienteSeleccionado.Password = Password;

            await _clienteRepository.UpdateAsync(ClienteSeleccionado);
            await Inicializa();

            return true;
        }

        private int _totalClientes; 
        public int TotalClientes 
        { 
            get => _totalClientes; 
            set => SetProperty(ref _totalClientes, value); 
        }

        // ============================================================
        // LIMPIAR CAMPOS
        // ============================================================

        public void LimpiarCampos()
        {
            Nombre = "";
            Apellido1 = "";
            Apellido2 = "";
            Dni = "";
            Telefono = "";
            Usuario = "";
            Password = "";
            ClienteSeleccionado = null;

            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellido1));
            OnPropertyChanged(nameof(Apellido2));
            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Telefono));
            OnPropertyChanged(nameof(Usuario));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(ClienteSeleccionado));
        }
    }
}
