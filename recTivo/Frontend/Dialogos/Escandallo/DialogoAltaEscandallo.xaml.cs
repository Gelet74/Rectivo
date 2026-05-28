using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoAltaEscandallo : Window
    {
        private readonly MVEscandallo _vm;

        public DialogoAltaEscandallo(MVEscandallo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.Inicializa();

            cmbArticuloFinal.SelectionChanged += CmbArticuloFinal_SelectionChanged;
        }

        private async void CmbArticuloFinal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var articuloElegido = cmbArticuloFinal.SelectedItem as recTivo.Backend.Modelos.Articulo;
            if (articuloElegido != null)
                await _vm.ValidarArticuloFinal(articuloElegido);
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

        private async void BtnCargarEscandallo_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.ArticuloFinal == null)
            {
                MensajeError.Mostrar("ALTA ESCANDALLO", "Debes seleccionar un artículo primero.");
                return;
            }
            await _vm.CargarEscandallo(_vm.ArticuloFinal.Codigo);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _ = _vm.LimpiarCampos();
        }

        private async void BtnAñadir_Click(object sender, RoutedEventArgs e)
        {
            if (!_vm.ArticuloFinalValido || _vm.ArticuloFinal == null)
            {
                MensajeError.Mostrar("ALTA ESCANDALLO", "Debes seleccionar un artículo final válido antes de añadir componentes.");
                return;
            }
            await _vm.AñadirComponente();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!_vm.ArticuloFinalValido || _vm.ArticuloFinal == null)
            {
                MensajeError.Mostrar("ALTA ESCANDALLO", "No puedes guardar: el artículo seleccionado ya tiene escandallo.");
                return;
            }
            await _vm.GuardarEscandallo();
        }
    }
}