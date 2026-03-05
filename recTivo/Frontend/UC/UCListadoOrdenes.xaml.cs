using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using recTivo.Frontend.Dialogos.VentanasInicio;

namespace recTivo.Frontend.UC
{
    public partial class UCListadoOrdenes : UserControl
    {
        private readonly MVOrden _vm;
        private bool _escapeEnCurso = false;

        public event Action? SolicitarCierre;

        public UCListadoOrdenes(MVOrden vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) =>
            {
                await _vm.InicializarListadoAsync();
                Focusable = true;
                Focus();
            };
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
            => _vm.LimpiarFiltros();

        private async void BtnRecargar_Click(object sender, RoutedEventArgs e)
            => await _vm.CargarOrdenesAsync();

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_escapeEnCurso) { e.Handled = true; return; }
                _escapeEnCurso = true;
                e.Handled = true;
                try
                {
                    var dialog = new ConfirmacionDialogo { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true && dialog.Confirmado)
                        SolicitarCierre?.Invoke();
                }
                finally { _escapeEnCurso = false; }
            }
            else base.OnPreviewKeyDown(e);
        }

        private async void BtnCerrarOrdenPT_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CerrarOrdenPTAsync();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

      
    }
}
