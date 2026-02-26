using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoModificarEscandallo : Window
    {
        private readonly MVEscandallo _vm;

        public DialogoModificarEscandallo(MVEscandallo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.Inicializa();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                var dialog = new ConfirmacionDialogo { Owner = this };
                bool? result = dialog.ShowDialog();
                if (result == true && dialog.Confirmado)
                    this.Close();
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private void TextBoxCantidad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (!decimal.TryParse(tb.Text,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture,
                        out _))
                {
                    MensajeError.Mostrar("CANTIDAD", "La cantidad introducida no es válida. Usa solo números y un separador decimal (coma o punto).");
                    tb.Dispatcher.BeginInvoke(() => tb.Focus());
                }
            }
        }

        private async void btnCargarEscandallo_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CargarEscandalloParaModificar();
        }

        private async void btnAñadirComponente_Click(object sender, RoutedEventArgs e)
        {
            await _vm.AñadirComponente();
        }

        private void btnActualizarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ComponenteEscandallo componente)
                _vm.ActualizarCantidad(componente);
        }

        private void btnQuitarComponente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ComponenteEscandallo componente)
                _vm.QuitarComponente(componente);
        }

        private async void btnGuardarEscandallo_Click(object sender, RoutedEventArgs e)
        {
            await _vm.GuardarYLimpiar();
        }
    }
}