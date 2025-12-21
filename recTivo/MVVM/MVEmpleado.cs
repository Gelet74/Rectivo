using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recTivo.MVVM
{
    public class MVEmpleado : MVBase
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly RolRepository _rolRepository;

        public MVEmpleado(EmpleadoRepository empleadoRepository, RolRepository rolRepository)
        {
            _empleadoRepository = empleadoRepository;
            _rolRepository = rolRepository;
        }

        // LISTADO
        private List<Empleado> _listaEmpleados;
        public List<Empleado> ListaEmpleados
        {
            get => _listaEmpleados;
            set => SetProperty(ref _listaEmpleados, value);
        }

        // ROLES
        private List<Rol> _listaRoles;
        public List<Rol> ListaRoles
        {
            get => _listaRoles;
            set => SetProperty(ref _listaRoles, value);
        }

        // CAMPOS DEL FORMULARIO
        public string Apellidos { get; set; }
        public string Nombre { get; set; }
        public string Dni { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? IdRol { get; set; }

        // MÉTODOS PRINCIPALES
        public async Task Inicializa()
        {
            ListaEmpleados = (List<Empleado>)await _empleadoRepository.GetAllAsync();
            ListaRoles = (List<Rol>)await _rolRepository.GetAllAsync();
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
        private int? _idEmpleadoSeleccionado;
        public int? IdEmpleadoSeleccionado
        {
            get => _idEmpleadoSeleccionado;
            set => SetProperty(ref _idEmpleadoSeleccionado, value);
        }


        public async Task<bool> GuardarAsync()
        {
            var empleado = new Empleado
            {
                Apellidos = Apellidos,
                Nombre = Nombre,
                Dni = Dni,
                Username = Username,
                Password = Password,
                IdRol = IdRol,
                Estado = "activo"
            };

            await _empleadoRepository.AddAsync(empleado);
            await Inicializa();
            LimpiarCampos();
            return true;
        }

        public void LimpiarCampos()
        {
            Apellidos = "";
            Nombre = "";
            Dni = "";
            Username = "";
            Password = "";
            IdRol = null;
        }
    }
}
