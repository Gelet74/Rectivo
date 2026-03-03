using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Ventas
{
    public partial class DialogoPedidos : Window
    {
        private readonly MVPedido _vm;
        public MVPedido ViewModel => _vm;

        public DialogoPedidos(
            PedidoRepository pedidoRepo,
            ArticuloRepository articuloRepo,
            EscandalloRepository escandalloRepo,
            ClienteRepository clienteRepo)
        {
            InitializeComponent();

            _vm = new MVPedido(pedidoRepo, articuloRepo, escandalloRepo, clienteRepo);
            DataContext = _vm;

            KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };

            Loaded += async (_, _) => await _vm.InicializarAsync();
        }

        // ── Tab changed ─────────────────────────────────────────────────
        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedIndex == 1)
                await _vm.CargarPedidosAsync();
        }

        // ── Checkbox artículo PT ────────────────────────────────────────
        private async void ChkPT_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.CheckBox)?.DataContext is Articulo art)
                await _vm.TogglePT(art, true);
        }

        private async void ChkPT_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.CheckBox)?.DataContext is Articulo art)
                await _vm.TogglePT(art, false);
        }

        // ── Edición cantidad en DataGrid ────────────────────────────────
        private void DgLineas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Notificar el total después de editar
            Dispatcher.BeginInvoke(() => _vm.NotificarTotalCambiado());
        }

        // ── Crear pedido ────────────────────────────────────────────────
        private async void BtnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CrearPedidoAsync();
        }

        // ── Cerrar pedido ───────────────────────────────────────────────
        private async void BtnCerrarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is FilaPedido fila)
                await _vm.CerrarPedidoAsync(fila);
            else
                MessageBox.Show("Selecciona un pedido de la lista.", "VENTAS",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}