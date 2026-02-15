using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using System;
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
            // Obtener el empleado desde App.xaml.cs
            string nombreEmpleado = "Empleado"; // Valor por defecto

            if (Application.Current is App app && app.EmpleadoActual != null)
            {
                nombreEmpleado = app.EmpleadoActual.NombreCompleto;
            }

            // Asignar DataContext usando la clase DashboardData
            DataContext = new DashboardData
            {
                UsuarioLogueado = nombreEmpleado,
                TotalArticulos = 0,
                TotalClientes = 0,
                TotalEmpleados = 0
            };
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

        private void BtnCambiarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el empleado actual
            if (Application.Current is App app)
            {
                app.EmpleadoActual = null;
            }

            // Cerrar el MainWindow y volver al Login
            var mainWindow = Window.GetWindow(this);

            var login = _serviceProvider.GetRequiredService<Login>();
            login.Show();

            mainWindow?.Close();
        }

        // ⭐ CLASE SIMPLE PARA EL DATACONTEXT
        private class DashboardData
        {
            public string UsuarioLogueado { get; set; }
            public int TotalArticulos { get; set; }
            public int TotalClientes { get; set; }
            public int TotalEmpleados { get; set; }
        }
    }
}