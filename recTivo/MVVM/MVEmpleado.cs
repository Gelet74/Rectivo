using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Windows.Data;

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

                var empleado = value ?? new Empleado();
#pragma warning disable CS8601
                SetProperty(ref _empleado, empleado);
#pragma warning restore CS8601

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

        // ============================================================
        // VISTA Y FILTROS
        // ============================================================

        public ListCollectionView? EmpleadosView { get; private set; }

        public List<string> NombreLista => ListaEmpleados
            .Select(e => e.Nombre).Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!).Distinct().OrderBy(n => n).ToList();

        public List<string> ApellidosLista => ListaEmpleados
            .Select(e => e.Apellidos).Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a!).Distinct().OrderBy(a => a).ToList();

        public List<string> RolLista => ListaEmpleados
            .Select(e => e.Rol?.NombreRol).Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!).Distinct().OrderBy(r => r).ToList();

        public List<string> EstadoLista => ListaEmpleados
            .Select(e => e.Estado).Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct().OrderBy(s => s).ToList();

        private string? _filtroNombre;
        public string? FiltroNombre
        {
            get => _filtroNombre;
            set { SetProperty(ref _filtroNombre, value); EmpleadosView?.Refresh(); }
        }

        private string? _filtroApellidos;
        public string? FiltroApellidos
        {
            get => _filtroApellidos;
            set { SetProperty(ref _filtroApellidos, value); EmpleadosView?.Refresh(); }
        }

        private string? _filtroRol;
        public string? FiltroRol
        {
            get => _filtroRol;
            set { SetProperty(ref _filtroRol, value); EmpleadosView?.Refresh(); }
        }

        private string? _filtroEstado;
        public string? FiltroEstado
        {
            get => _filtroEstado;
            set { SetProperty(ref _filtroEstado, value); EmpleadosView?.Refresh(); }
        }

        private bool FiltrarEmpleados(object obj)
        {
            if (obj is not Empleado e) return false;

            bool coincideNombre = string.IsNullOrWhiteSpace(FiltroNombre) ||
                (e.Nombre?.Contains(FiltroNombre, StringComparison.OrdinalIgnoreCase) ?? false);
            bool coincideApellidos = string.IsNullOrWhiteSpace(FiltroApellidos) ||
                (e.Apellidos?.Contains(FiltroApellidos, StringComparison.OrdinalIgnoreCase) ?? false);
            bool coincideRol = string.IsNullOrWhiteSpace(FiltroRol) ||
                (e.Rol?.NombreRol?.Contains(FiltroRol, StringComparison.OrdinalIgnoreCase) ?? false);
            bool coincideEstado = string.IsNullOrWhiteSpace(FiltroEstado) ||
                (e.Estado?.Contains(FiltroEstado, StringComparison.OrdinalIgnoreCase) ?? false);

            return coincideNombre && coincideApellidos && coincideRol && coincideEstado;
        }

        // ============================================================
        // NOMBRES PARA COMBOBOX
        // ============================================================

        private List<string> _nombresEmpleados = new();
        public List<string> NombresEmpleados
        {
            get => _nombresEmpleados;
            set => SetProperty(ref _nombresEmpleados, value);
        }

        private string? _nombreEmpleadoSeleccionado;
        public string? NombreEmpleadoSeleccionado
        {
            get => _nombreEmpleadoSeleccionado;
            set
            {
                SetProperty(ref _nombreEmpleadoSeleccionado, value);
                EmpleadoSeleccionado = ListaEmpleados.FirstOrDefault(e => e.NombreCompleto == value);
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MVEmpleado(EmpleadoRepository empleadoRepository, RolRepository rolRepository)
        {
            _empleadoRepository = empleadoRepository;
            _rolRepository = rolRepository;

            _empleado = new Empleado();
            _empleado.PropertyChanged += OnEmpleadoPropertyChanged;

            _listaEmpleados = new List<Empleado>();
            _listaRoles = new List<Rol>();
            _empleadoSeleccionado = new Empleado();
            _rolSeleccionado = new Rol();
            _nombre = string.Empty;
            _apellidos = string.Empty;
            _dni = string.Empty;
            _username = string.Empty;
            _password = string.Empty;
        }

        // ============================================================
        // LISTADOS
        // ============================================================

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

        // ============================================================
        // SELECCIÓN
        // ============================================================

        private Empleado? _empleadoSeleccionado;
        public Empleado? EmpleadoSeleccionado
        {
            get => _empleadoSeleccionado;
            set => SetProperty(ref _empleadoSeleccionado, value);
        }

        private Rol? _rolSeleccionado;
        public Rol? RolSeleccionado
        {
            get => _rolSeleccionado;
            set { SetProperty(ref _rolSeleccionado, value); IdRol = value?.Id; }
        }

        // ============================================================
        // CAMPOS DEL FORMULARIO
        // ============================================================

        private string? _nombre;
        public string? Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string? _apellidos;
        public string? Apellidos
        {
            get => _apellidos;
            set => SetProperty(ref _apellidos, value);
        }

        private string? _dni;
        public string? Dni
        {
            get => _dni;
            set => SetProperty(ref _dni, value);
        }

        private string? _username;
        public string? Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string? _password;
        public string? Password
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

        // ============================================================
        // INICIALIZAR
        // ============================================================

        public async Task Inicializa()
        {
            ListaEmpleados = (await _empleadoRepository.GetAllAsync()).ToList();
            ListaRoles = (await _rolRepository.GetAllAsync()).ToList();

            NombresEmpleados = ListaEmpleados
                .Select(e => e.NombreCompleto)
                .ToList();

            EmpleadosView = new ListCollectionView(ListaEmpleados);
            EmpleadosView.Filter = FiltrarEmpleados;

            OnPropertyChanged(nameof(EmpleadosView));
            OnPropertyChanged(nameof(NombreLista));
            OnPropertyChanged(nameof(ApellidosLista));
            OnPropertyChanged(nameof(RolLista));
            OnPropertyChanged(nameof(EstadoLista));
        }

        public void LimpiarFiltros()
        {
            FiltroNombre = "";
            FiltroApellidos = "";
            FiltroRol = "";
            FiltroEstado = "";
        }

        // ============================================================
        // ALTA
        // ============================================================

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

        // ============================================================
        // CARGAR EMPLEADO SELECCIONADO
        // ============================================================

        public Task<bool> CargarEmpleadoSeleccionadoAsync()
        {
            if (EmpleadoSeleccionado == null)
                return Task.FromResult(false);

            Nombre = EmpleadoSeleccionado.Nombre;
            Apellidos = EmpleadoSeleccionado.Apellidos;
            Dni = EmpleadoSeleccionado.Dni;
            Username = EmpleadoSeleccionado.Username;
            Password = EmpleadoSeleccionado.Password;
            Estado = EmpleadoSeleccionado.Estado;
            RolSeleccionado = ListaRoles.FirstOrDefault(r => r.Id == EmpleadoSeleccionado.Rol?.Id);

            OnPropertyChanged(nameof(Nombre));
            OnPropertyChanged(nameof(Apellidos));
            OnPropertyChanged(nameof(Dni));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(Estado));
            OnPropertyChanged(nameof(RolSeleccionado));

            return Task.FromResult(true);
        }

        // ============================================================
        // MODIFICAR
        // ============================================================

        public async Task<bool> ModificarEmpleadoAsync()
        {
            if (EmpleadoSeleccionado == null)
                return false;

            EmpleadoSeleccionado.Nombre = Nombre;
            EmpleadoSeleccionado.Apellidos = Apellidos;
            EmpleadoSeleccionado.Dni = Dni;
            EmpleadoSeleccionado.Username = Username;
            if (EmpleadoSeleccionado.Password != Password)
            {
                EmpleadoSeleccionado.Password = BCrypt.Net.BCrypt.HashPassword(Password);
            }

            EmpleadoSeleccionado.Estado = Estado;
            EmpleadoSeleccionado.IdRol = RolSeleccionado?.Id;

            await _empleadoRepository.UpdateAsync(EmpleadoSeleccionado);
            await Inicializa();

            return true;
        }

        // ============================================================
        // LIMPIAR CAMPOS
        // ============================================================

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

        // ============================================================
        // ELIMINAR
        // ============================================================

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
    }
}