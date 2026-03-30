using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
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

        private bool _escapeEnCurso = false;

        public DialogoPedidos(MVPedido vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = vm;

            Loaded += async (_, _) => await _vm.InicializarAsync();
        }

        // ───────────────────────────────────────────────────────────────
        // ESC con confirmación
        // ───────────────────────────────────────────────────────────────
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_escapeEnCurso)
                {
                    e.Handled = true;
                    return;
                }
                _escapeEnCurso = true;
                e.Handled = true;
                try
                {
                    var dialog = new ConfirmacionDialogo { Owner = this };
                    bool? result = dialog.ShowDialog();
                    if (result == true && dialog.Confirmado)
                        this.Close();
                }
                finally
                {
                    _escapeEnCurso = false;
                }
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        // ───────────────────────────────────────────────────────────────
        // Checkbox artículo PT
        // ───────────────────────────────────────────────────────────────
        private async void ChkPT_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is Articulo art)
                await _vm.TogglePT(art, true);
        }

        private async void ChkPT_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is Articulo art)
                await _vm.TogglePT(art, false);
        }

        // ───────────────────────────────────────────────────────────────
        // Edición cantidad en DataGrid
        // ───────────────────────────────────────────────────────────────
        private void DgLineas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(() => _vm.NotificarTotalCambiado());
        }

        // ───────────────────────────────────────────────────────────────
        // Crear pedido
        // ───────────────────────────────────────────────────────────────
        private async void BtnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CrearPedidoAsync();
        }

        // ───────────────────────────────────────────────────────────────
        // Cerrar pedido
        // ───────────────────────────────────────────────────────────────
        private async void BtnCerrarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is FilaPedido fila)
            {
                await _vm.CerrarPedidoAsync(fila);
            }
            else
            {
                MensajeInformacion.Mostrar(
                    "VENTAS", "Selecciona un pedido de la lista haciendo clic sobre él");
            }
        }
    }
}
