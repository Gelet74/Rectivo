using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Ordenes
{
    public partial class DialogoProcesarOrden : Window
    {
        private readonly MVOrden _vm;

        public DialogoProcesarOrden(MVOrden vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            // Seleccionar "Solo PS" por defecto al abrir
            rbSoloPT.IsChecked = true;

            Loaded += async (_, __) => await _vm.InicializarAsync();
        }

        // ── RadioButtons: gestionados en code-behind para evitar converter inverso ──

        private void RbSoloPT_Checked(object sender, RoutedEventArgs e)
        {
            _vm.IncluirPT = false;
            // Recalcular preview si ya estaba visible para reflejar el cambio
            if (_vm.PreviewVisible)
                _ = _vm.CalcularPreviewAsync();
        }

        private void RbConPT_Checked(object sender, RoutedEventArgs e)
        {
            _vm.IncluirPT = true;
            if (_vm.PreviewVisible)
                _ = _vm.CalcularPreviewAsync();
        }

        private void TxtCantidad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (!decimal.TryParse(tb.Text,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture,
                        out decimal val) || val <= 0)
                {
                    MensajeError.Mostrar("CANTIDAD", "Introduce un número válido mayor que 0.");
                    tb.Text = "1";
                    _vm.CantidadFabricar = 1;
                }
            }
        }

        private async void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CalcularPreviewAsync();
        }

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is not App app || app.EmpleadoActual == null)
            {
                MensajeError.Mostrar("SESIÓN", "No hay ningún empleado con sesión iniciada.");
                return;
            }

            bool ok = await _vm.GenerarOrdenesAsync(app.EmpleadoActual);
            if (ok)
                Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                var dialog = new ConfirmacionDialogo { Owner = this };
                if (dialog.ShowDialog() == true && dialog.Confirmado)
                    Close();
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }
    }
}
