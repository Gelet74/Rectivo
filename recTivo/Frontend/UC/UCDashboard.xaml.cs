using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using recTivo.Frontend.Dialogos.Ordenes;
using recTivo.Frontend.Dialogos.Ventas;
using System.Windows;
using System.Windows.Controls;

namespace recTivo.Frontend.UC
{
    /// <summary>
    /// Lógica de interacción para UCDashboard.xaml
    /// </summary>
    public partial class UCDashboard : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        public UCDashboard(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
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

        private void BtnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            _serviceProvider.GetService<DialogoCrearPedido>()?.ShowDialog();
        }

    }
}
