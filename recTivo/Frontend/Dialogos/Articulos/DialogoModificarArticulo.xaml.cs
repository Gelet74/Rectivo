using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoModificarArticulo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoModificarArticulo(MVArticulo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();

            // ELIMINADO: registro duplicado de PreviewKeyDown
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

            // ELIMINADO: DialogoModificarArticulo_PreviewKeyDown — era duplicado
            // que cerraba sin confirmación, contradiciendo a OnPreviewKeyDown
        }

        private async void btnCargarArticulo_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.ArticuloSeleccionado == null)
            {
                MensajeError.Mostrar("MODIFICAR ARTÍCULO", "Debes seleccionar un artículo primero.");
                return;
            }

            var encontrado = await _vm.CargarArticuloSeleccionadoAsync();

            if (!encontrado)
            {
                MensajeError.Mostrar("MODIFICAR ARTÍCULO", "No se ha encontrado ningún artículo con ese código.");
                panelDatos.Visibility = Visibility.Collapsed;
                return;
            }

            panelDatos.Visibility = Visibility.Visible;
            panelDatos.UpdateLayout();
        }

        private async void btnModificarArticulo_Click(object sender, RoutedEventArgs e)
        {
            var ok = await _vm.ModificarAsync();

            if (ok)
                this.Close();
        }
    }
}