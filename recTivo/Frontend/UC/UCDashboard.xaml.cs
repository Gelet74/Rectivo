using Microsoft.Extensions.DependencyInjection;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using recTivo.Frontend.Dialogos.Ordenes;
using recTivo.Frontend.Dialogos.Ventas;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace recTivo.Frontend.UC
{
    public partial class UCDashboard : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        public UCDashboard(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            Loaded += UCDashboard_Loaded;
        }

        private void UCDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            string nombreEmpleado = "Empleado";

            if (Application.Current is App app && app.EmpleadoActual != null)
                nombreEmpleado = app.EmpleadoActual.NombreCompleto;

            var context = _serviceProvider.GetRequiredService<RectivoContext>();

            DataContext = new DashboardData
            {
                UsuarioLogueado = nombreEmpleado,
                TotalArticulos = context.Articulos.Count(),
                TotalClientes = context.Clientes.Count(),
                TotalOrdenes = context.Orden.Count(),
                TotalEmpleados = context.Empleados.Count()
            };

            AplicarRestriccionesDashboard();
        }

        private void AplicarRestriccionesDashboard()
        {
            if (Application.Current is not App app || app.EmpleadoActual?.Rol == null)
                return;

            var permisos = app.EmpleadoActual.Rol.Permisos
                .Select(p => p.NombrePermiso)
                .ToHashSet();

            // Administrador → todo visible
            if (app.EmpleadoActual.Rol.NombreRol == "Administrador" || permisos.Count == 0)
                return;

            // Crear artículo
            btnCrearArticulo.Visibility = permisos.Contains("Crear artículos")
                ? Visibility.Visible : Visibility.Collapsed;

            // Crear cliente → solo admin (no hay permiso específico todavía)
            btnCrearCliente.Visibility = Visibility.Collapsed;

            // Crear empleado
            btnCrearEmpleado.Visibility = permisos.Contains("Ver usuarios")
                ? Visibility.Visible : Visibility.Collapsed;

            // Crear escandallo
            btnCrearEscandallo.Visibility = permisos.Contains("Crear escandallos")
                ? Visibility.Visible : Visibility.Collapsed;

            // Procesar orden
            btnProcesarOrden.Visibility = permisos.Contains("Registrar movimientos de stock")
                ? Visibility.Visible : Visibility.Collapsed;

            // Crear pedido → solo cuando haya ventas implementadas
            btnCrearPedido.Visibility = permisos.Contains("Gestionar ventas")
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCrearArticulo_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoAltaArticulo>()?.ShowDialog();
        }

        private void BtnCrearCliente_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoAltaCliente>()?.ShowDialog();
        }

        private void BtnCrearEmpleado_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoAltaEmpleado>()?.ShowDialog();
        }

        private void BtnCrearEscandallo_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoAltaEscandallo>()?.ShowDialog();
        }

        private void BtnProcesarOrden_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoProcesarOrden>()?.ShowDialog();
        }

        private void btnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoPedidos>()?.ShowDialog();
        }

        private void BtnCambiarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
                app.EmpleadoActual = null;

            var mainWindow = Window.GetWindow(this);

            var login = _serviceProvider.GetRequiredService<Login>();
            login.Show();

            mainWindow?.Close();
        }

        private class DashboardData : INotifyPropertyChanged
        {
            private string _usuarioLogueado;
            public string UsuarioLogueado
            {
                get => _usuarioLogueado;
                set { _usuarioLogueado = value; OnPropertyChanged(nameof(UsuarioLogueado)); }
            }           

            private int _totalArticulos;
            public int TotalArticulos
            {
                get => _totalArticulos;
                set { _totalArticulos = value; OnPropertyChanged(nameof(TotalArticulos)); }
            }

            private int _totalClientes;
            public int TotalClientes
            {
                get => _totalClientes;
                set { _totalClientes = value; OnPropertyChanged(nameof(TotalClientes)); }
            }


            private int _totalOrdenes;
            public int TotalOrdenes
            {
                get => _totalOrdenes;
                set { _totalOrdenes = value; OnPropertyChanged(nameof(TotalOrdenes)); }
            }

            private int _totalEmpleados;
            public int TotalEmpleados
            {
                get => _totalEmpleados;
                set { _totalEmpleados = value; OnPropertyChanged(nameof(TotalEmpleados)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

       
    }
}
