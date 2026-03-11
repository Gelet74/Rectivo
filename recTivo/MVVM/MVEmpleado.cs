using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;

namespace recTivo.MVVM
{
    public class MVEmpleado : MVBase
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly RolRepository _rolRepository;

        private Empleado _empleado;
        public Empleado Empleado
        {
            get => _empleado ?? (_empleado = new Empleado());
            set
            {
                if (_empleado != null)
                    _empleado.PropertyChanged -= OnEmpleadoPropertyChanged;

                SetProperty(ref _empleado, value);

                if (_empleado != null)
                    _empleado.PropertyChanged += OnEmpleadoPropertyChanged;
            }
        }

        public override bool HasNoErrors =>
            !HasErrors &&
            !string.IsNullOrWhiteSpace(Empleado.Nombre) &&
            !string.IsNullOrWhiteSpace(Empleado.Apellidos) &&
            !string.IsNullOrWhiteSpace(Empleado.Username) &&
            !string.IsNullOrWhiteSpace(Empleado.Password) &&
            Empleado.Rol != null;

        private void OnEmpleadoPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasNoErrors));
        }

        public MVEmpleado(EmpleadoRepository empleadoRepository, RolRepository rolRepository)
        {
            _empleadoRepository = empleadoRepository;
            _rolRepository = rolRepository;

            // Inicializamos Empleado aquí para suscribirnos desde el principio
            _empleado = new Empleado();
            _empleado.PropertyChanged += OnEmpleadoPropertyChanged;
        }

        private List<Empleado> _listaEmpleados;
        public List<Empleado> ListaEmpleados
        {
            get => _listaEmpleados;
            set => SetProperty(ref _listaEmpleados, value);
        }

        private List<Rol> _listaRoles;
        public List<Rol> ListaRoles
        {
            get => _listaRoles;
            set => SetProperty(ref _listaRoles, value);
        }

        public List<string> Estados { get; } = new() { "activo", "inactivo" };

        private Empleado _empleadoSeleccionado;
        public Empleado EmpleadoSeleccionado
        {
            get => _empleadoSeleccionado;
            set => SetProperty(ref _empleadoSeleccionado, value);
        }

        private Rol _rolSeleccionado;
        public Rol RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                SetProperty(ref _rolSeleccionado, value);
                IdRol = value?.Id;
            }
        }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _apellidos;
        public string Apellidos
        {
            get => _apellidos;
            set => SetProperty(ref _apellidos, value);
        }

        private string _dni;
        public string Dni
        {
            get => _dni;
            set => SetProperty(ref _dni, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _estado = "activo";
        public string Estado
        {
            get => _estado;
            set => SetProperty(ref _estado, value);
        }

        private int? _idRol;
        public int? IdRol
        {
            get => _idRol;
            set => SetProperty(ref _idRol, value);
        }

        public async Task Inicializa()
        {
            ListaEmpleados = (await _empleadoRepository.GetAllAsync()).ToList();
            ListaRoles = (await _rolRepository.GetAllAsync()).ToList();
        }

        public async Task<bool> GuardarAsync()
        {
            var empleado = new Empleado
            {
                Apellidos = Empleado.Apellidos,
                Nombre = Empleado.Nombre,
                Dni = Empleado.Dni,
                Username = Empleado.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(Password),
                IdRol = Empleado.Rol?.Id,
                Estado = Estado
            };

            await _empleadoRepository.AddAsync(empleado);
            await Inicializa();
            LimpiarCampos();
            return true;
        }

        public void LimpiarCampos()
        {
            Empleado = new Empleado();
            Estado = "activo";
            IdRol = null;
            RolSeleccionado = null;
            EmpleadoSeleccionado = null;

            OnPropertyChanged(nameof(Estado));
            OnPropertyChanged(nameof(RolSeleccionado));
            OnPropertyChanged(nameof(EmpleadoSeleccionado));
            OnPropertyChanged(nameof(HasNoErrors));
        }

        public async Task<bool> EliminarAsync(int idEmpleado)
        {
            try
            {
                await _empleadoRepository.DeleteAsync(idEmpleado);
                await Inicializa();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CargarEmpleadoSeleccionadoAsync()
        {
            if (EmpleadoSeleccionado == null)
                return false;

            Nombre = EmpleadoSeleccionado.Nombre;
            Apellidos = EmpleadoSeleccionado.Apellidos;
            Dni = EmpleadoSeleccionado.Dni;
            Username = EmpleadoSeleccionado.Username;
            Password = EmpleadoSeleccionado.Password;
            Estado = EmpleadoSeleccionado.Estado;
            RolSeleccionado = EmpleadoSeleccionado.Rol;

            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellidos));
            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(Estado));
            OnPropertyChanged(nameof(RolSeleccionado));

            return true;
        }

        public async Task<bool> ModificarEmpleadoAsync()
        {
            if (EmpleadoSeleccionado == null)
                return false;

            EmpleadoSeleccionado.Nombre = Nombre;
            EmpleadoSeleccionado.Apellidos = Apellidos;
            EmpleadoSeleccionado.Dni = Dni;
            EmpleadoSeleccionado.Username = Username;
            EmpleadoSeleccionado.Password = Password;
            EmpleadoSeleccionado.Estado = Estado;
            EmpleadoSeleccionado.IdRol = RolSeleccionado?.Id;

            await _empleadoRepository.UpdateAsync(EmpleadoSeleccionado);
            await Inicializa();

            return true;
        }
    }
}