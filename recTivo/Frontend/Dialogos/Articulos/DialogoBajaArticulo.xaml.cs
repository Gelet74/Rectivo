using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoBajaArticulo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoBajaArticulo(MVArticulo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private async void btnBajaArticulo_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _vm.BajaPorCodigoAsync();
            if (ok)
            {
                // CORREGIDO: era MensajeError, debe ser MensajeInformacion
                MensajeInformacion.Mostrar("ÉXITO", "Artículo dado de baja correctamente");
                this.Close();
            }
            else
            {
                MensajeError.Mostrar("Error", "No se encontró el artículo con ese código");
            }
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
    }
}