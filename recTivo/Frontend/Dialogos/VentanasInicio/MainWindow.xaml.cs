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
using recTivo.Frontend.UC;
using recTivo.MVVM;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private UIElement _dashboardInicial;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            panelPrincipal.Children.Add(_serviceProvider.GetService<UCDashboard>());

            DataContext = this;
            _ = CargarTotalesAsync();

            AplicarRestriccionesRol();
        }

        private void AplicarRestriccionesRol()
        {
            if (Application.Current is not App app || app.EmpleadoActual?.Rol == null)
                return;

            var permisos = app.EmpleadoActual.Rol.Permisos
                .Select(p => p.NombrePermiso)
                .ToHashSet();

            // Si no tiene ningún permiso configurado es Administrador o dev → todo visible
            if (permisos.Count == 0) return;

            bool esAdmin = app.EmpleadoActual.Rol.NombreRol == "Administrador";
            if (esAdmin) return;

            // ── Almacén ───────────────────────────────────────────────────
            bool verAlmacen = permisos.Contains("Hacer movimientos de almacen") ||
                              permisos.Contains("Registrar movimientos de stock");
            expAlmacen.Visibility = verAlmacen ? Visibility.Visible : Visibility.Collapsed;

            // ── Artículos ─────────────────────────────────────────────────
            bool verArticulos = permisos.Contains("Ver artículos") ||
                                permisos.Contains("Crear artículos") ||
                                permisos.Contains("Editar artículos") ||
                                permisos.Contains("Eliminar artículos");
            expArticulos.Visibility = verArticulos ? Visibility.Visible : Visibility.Collapsed;

            // Ocultar items específicos dentro de Artículos
            if (verArticulos)
            {
                foreach (ListViewItem item in articulos.Items)
                {
                    string content = item.Content?.ToString() ?? "";
                    if (content == "Dar de alta")
                        item.Visibility = permisos.Contains("Crear artículos") ? Visibility.Visible : Visibility.Collapsed;
                    else if (content == "Dar de baja")
                        item.Visibility = permisos.Contains("Eliminar artículos") ? Visibility.Visible : Visibility.Collapsed;
                    else if (content == "Modificar")
                        item.Visibility = permisos.Contains("Editar artículos") ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // ── Clientes ──────────────────────────────────────────────────
            expClientes.Visibility = Visibility.Collapsed; // Solo admin/administrativo

            // ── Empleados ─────────────────────────────────────────────────
            bool verEmpleados = permisos.Contains("Ver usuarios") || permisos.Contains("Gestionar roles");
            expEmpleados.Visibility = verEmpleados ? Visibility.Visible : Visibility.Collapsed;

            // ── Escandallos ───────────────────────────────────────────────
            bool verEscandallos = permisos.Contains("Crear escandallos") || permisos.Contains("Editar escandallos");
            expEscandallos.Visibility = verEscandallos ? Visibility.Visible : Visibility.Collapsed;

            // ── Órdenes ───────────────────────────────────────────────────
            bool puedeProcesat = permisos.Contains("Registrar movimientos de stock");
            bool puedeCerrar = permisos.Contains("Cerrar fases") || permisos.Contains("Cerrar ordenes");
            bool verOrdenes = puedeProcesat || puedeCerrar;
            expOrdenes.Visibility = verOrdenes ? Visibility.Visible : Visibility.Collapsed;

            if (verOrdenes)
            {
                foreach (ListViewItem item in ordenes.Items)
                {
                    string itemContent = item.Content?.ToString() ?? "";
                    if (itemContent == "Procesar orden")
                        item.Visibility = puedeProcesat ? Visibility.Visible : Visibility.Collapsed;
                    else if (itemContent is "Cerrar orden" or "Listar órdenes")
                        item.Visibility = puedeCerrar ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // ── Ventas ────────────────────────────────────────────────────
            bool verVentas = permisos.Contains("Gestionar ventas");
            expVentas.Visibility = verVentas ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MostrarDashboard()
        {
            panelPrincipal.Children.Clear();
            _dashboardInicial = _serviceProvider.GetRequiredService<UCDashboard>();
            panelPrincipal.Children.Add(_dashboardInicial);
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

        private int _totalEmpleados;
        public int TotalEmpleados
        {
            get => _totalEmpleados;
            set { _totalEmpleados = value; OnPropertyChanged(nameof(TotalEmpleados)); }
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
                    {
                        panelPrincipal.Children.Clear();
                        var uc = _serviceProvider.GetService<UCListadoArticulos>();
                        uc.SolicitarCierre += () =>
                        {
                            panelPrincipal.Children.Remove(uc);
                            MostrarDashboard();
                        };
                        panelPrincipal.Children.Add(uc);
                        break;
                    }
            }

            articulos.SelectedItem = null;
        }

        private void clientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clientes.SelectedItem is not ListViewItem item) return;

            switch (item.Content.ToString())
            {
                case "Dar de alta":
                    // CORREGIDO: usar el contenedor DI en lugar de new
                    _serviceProvider.GetService<DialogoAltaCliente>()?.ShowDialog();
                    break;
                case "Modificar":
                    _serviceProvider.GetService<DialogoModificarCliente>()?.ShowDialog();
                    break;
                case "Listar clientes":
                    {
                        panelPrincipal.Children.Clear();
                        var uc = _serviceProvider.GetService<UCListadoClientes>();
                        uc.SolicitarCierre += () =>
                        {
                            panelPrincipal.Children.Remove(uc);
                            MostrarDashboard();
                        };
                        panelPrincipal.Children.Add(uc);
                        break;
                    }
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
                case "Listar órdenes":
                    {
                        panelPrincipal.Children.Clear();
                        var uc = _serviceProvider.GetRequiredService<UCListadoOrdenes>();
                        uc.SolicitarCierre += () =>
                        {
                            panelPrincipal.Children.Remove(uc);
                            MostrarDashboard();
                        };
                        panelPrincipal.Children.Add(uc);
                        break;
                    }
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
            }

            ventas.SelectedItem = null;
        }

        private void salir_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = _serviceProvider.GetRequiredService<ConfirmacionDialogo>();
            dialogo.Owner = this;

            if (dialogo.ShowDialog() == true)
                Application.Current.Shutdown();
        }

        private void Menu_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}