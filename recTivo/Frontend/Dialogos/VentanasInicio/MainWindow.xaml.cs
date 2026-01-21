using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Backend.Repos;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using recTivo.Frontend.Dialogos.Ordenes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Frontend.Dialogos.Ventas;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            DataContext = this;
            _ = CargarTotalesAsync();
        }

        // ============================
        // PROPIEDADES DE DASHBOARD
        // ============================

        private int _totalArticulos;
        public int TotalArticulos
        {
            get => _totalArticulos;
            set
            {
                _totalArticulos = value;
                OnPropertyChanged(nameof(TotalArticulos));
            }
        }

        private int _totalClientes;
        public int TotalClientes
        {
            get => _totalClientes;
            set
            {
                _totalClientes = value;
                OnPropertyChanged(nameof(TotalClientes));
            }
        }

        private int _totalEmpleados;
        public int TotalEmpleados
        {
            get => _totalEmpleados;
            set
            {
                _totalEmpleados = value;
                OnPropertyChanged(nameof(TotalEmpleados));
            }
        }

        private async Task CargarTotalesAsync()
        {
            var articuloRepo = _serviceProvider.GetService<ArticuloRepository>();
            var clienteRepo = _serviceProvider.GetService<ClienteRepository>();
            var empleadoRepo = _serviceProvider.GetService<EmpleadoRepository>();

            TotalArticulos = (await articuloRepo.GetAllAsync()).Count();
            TotalClientes = (await clienteRepo.GetAllAsync()).Count();
            TotalEmpleados = (await empleadoRepo.GetAllAsync()).Count();
        }

        // ============================
        // EVENTOS DE MENÚ
        // ============================

        private void almacen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (almacen.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Entradas almacén":
                    _serviceProvider.GetService<DialogoEntradaAlmacen>()?.ShowDialog();
                    break;
                case "Salidas almacén":
                    _serviceProvider.GetService<DialogoSalidaAlmacen>()?.ShowDialog();
                    break;
            }

            almacen.SelectedItem = null;
        }

        private void articulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (articulos.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Dar de alta":
                    _serviceProvider.GetService<DialogoAltaArticulo>()?.ShowDialog();
                    break;
                case "Dar de baja":
                    _serviceProvider.GetService<DialogoBajaArticulo>()?.ShowDialog();
                    break;
                case "Modificar":
                    _serviceProvider.GetService<DialogoModificarArticulo>()?.ShowDialog();
                    break;
                case "Listar artículos":
                    _serviceProvider.GetService<DialogoListarArticulo>()?.ShowDialog();
                    break;
            }

            articulos.SelectedItem = null;
        }

        private void clientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clientes.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Dar de alta":
                    _serviceProvider.GetService<DialogoAltaCliente>()?.ShowDialog();
                    break;
                case "Modificar":
                    _serviceProvider.GetService<DialogoModificarCliente>()?.ShowDialog();
                    break;
                case "Listar clientes":
                    _serviceProvider.GetService<DialogoConsultaCliente>()?.ShowDialog();
                    break;
            }

            clientes.SelectedItem = null;
        }

        private void empleados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (empleados.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Dar de alta":
                    _serviceProvider.GetService<DialogoAltaEmpleado>()?.ShowDialog();
                    break;
                case "Modificar":
                    _serviceProvider.GetService<DialogoModificarEmpleado>()?.ShowDialog();
                    break;
                case "Listar empleados":
                    _serviceProvider.GetService<DialogoConsultaEmpleado>()?.ShowDialog();
                    break;
            }

            empleados.SelectedItem = null;
        }

        private void escandallos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (escandallos.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Dar de alta":
                    _serviceProvider.GetService<DialogoAltaEscandallo>()?.ShowDialog();
                    break;
                case "Modificar":
                    _serviceProvider.GetService<DialogoModificarEscandallo>()?.ShowDialog();
                    break;
                case "Listar escandallo":
                    _serviceProvider.GetService<DialogoListarEscandallo>()?.ShowDialog();
                    break;
            }

            escandallos.SelectedItem = null;
        }

        private void ordenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ordenes.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Procesar orden":
                    _serviceProvider.GetService<DialogoProcesarOrden>()?.ShowDialog();
                    break;
                //case "Cerrar orden":
                    //_serviceProvider.GetService<DialogoCerrarOrden>()?.ShowDialog();
                    //break;
                //case "Listar órdenes":
                    //_serviceProvider.GetService<DialogoListarOrden>()?.ShowDialog();
                    //break;
            }

            ordenes.SelectedItem = null;
        }

        private void ventas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ventas.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Crear pedido":
                    _serviceProvider.GetService<DialogoCrearPedido>()?.ShowDialog();
                    break;
                //case "Cerrar pedido":
                    //_serviceProvider.GetService<DialogoCerrarPedido>()?.ShowDialog();
                    //break;
                //case "Listar pedidos":
                    //_serviceProvider.GetService<DialogoListarPedido>()?.ShowDialog();
                    //break;
            }

            ventas.SelectedItem = null;
        }

        // ============================
        // BOTONES DE ACCESO RÁPIDO
        // ============================

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

        private void salir_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = _serviceProvider.GetService<ConfirmacionDialogo>();
            dialogo.Owner = this;

            if (dialogo.ShowDialog() == true)
                Application.Current.Shutdown();
        }

        private void Menu_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                e.Handled = true;
        }

        // ============================
        // INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
