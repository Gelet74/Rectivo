using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.VentanasInicio;
using System;
using System.Windows;
using System.Windows.Controls;

namespace recTivo
{
    public partial class MainWindow : MetroWindow
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private void almacen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (almacen.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Entradas almacén":
                    var entrada = _serviceProvider.GetService<DialogoEntradaAlmacen>();
                    entrada.Owner = this;
                    entrada.ShowDialog();
                    break;

                case "Salidas almacén":
                    var salida = _serviceProvider.GetService<DialogoSalidaAlmacen>();
                    salida.Owner = this;
                    salida.ShowDialog();
                    break;
            }

            almacen.SelectedItem = null;
        }

        private void articulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (articulos.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Dar de alta":
                    var altaArticulo = _serviceProvider.GetService<DialogoAltaArticulo>();
                    altaArticulo.Owner = this;
                    altaArticulo.ShowDialog();
                    break;

                case "Dar de baja":
                    var bajaArticulo = _serviceProvider.GetService<DialogoBajaArticulo>();
                    bajaArticulo.Owner = this;
                    bajaArticulo.ShowDialog();
                    break;

                case "Modificar":
                    var modificarArticulo = _serviceProvider.GetService<DialogoModificarArticulo>();
                    modificarArticulo.Owner = this;
                    modificarArticulo.ShowDialog();
                    break;

                case "Listar artículos":
                    var listarArticulo = _serviceProvider.GetService<DialogoListarArticulo>();
                    listarArticulo.Owner = this;
                    listarArticulo.ShowDialog();
                    break;
            }

            articulos.SelectedItem = null;
        }

        private void clientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clientes.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Dar de alta":
                    var altaCliente = _serviceProvider.GetService<DialogoAltaClientes>();
                    altaCliente.Owner = this;
                    altaCliente.ShowDialog();
                    break;

                case "Dar de baja":
                    var bajaCliente = _serviceProvider.GetService<DialogoBajaCliente>();
                    bajaCliente.Owner = this;
                    bajaCliente.ShowDialog();
                    break;

                case "Modificar":
                    var modificarCliente = _serviceProvider.GetService<DialogoModificarCliente>();
                    modificarCliente.Owner = this;
                    modificarCliente.ShowDialog();
                    break;

                case "Listar clientes":
                    var listarCliente = _serviceProvider.GetService<DialogoConsultaCliente>();
                    listarCliente.Owner = this;
                    listarCliente.ShowDialog();
                    break;
            }

            clientes.SelectedItem = null;
        }

        private void empleados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (empleados.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Dar de alta":
                    var altaEmpleado = _serviceProvider.GetService<DialogoAltaEmpleado>();
                    altaEmpleado.Owner = this;
                    altaEmpleado.ShowDialog();
                    break;

                case "Dar de baja":
                    var bajaEmpleado = _serviceProvider.GetService<DialogoBajaEmpleado>();
                    bajaEmpleado.Owner = this;
                    bajaEmpleado.ShowDialog();
                    break;

                case "Modificar":
                    var modificarEmpleado = _serviceProvider.GetService<DialogoModificarEmpleado>();
                    modificarEmpleado.Owner = this;
                    modificarEmpleado.ShowDialog();
                    break;

                case "Listar empleados":
                    var listarEmpleado = _serviceProvider.GetService<DialogoConsultaEmpleado>();
                    listarEmpleado.Owner = this;
                    listarEmpleado.ShowDialog();
                    break;
            }

            empleados.SelectedItem = null;
        }

        private void salir_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = _serviceProvider.GetService<ConfirmacionDialogo>();
            dialogo.Owner = this;

            bool? resultado = dialogo.ShowDialog();

            if (resultado == true)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
