using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Clientes
{
    public partial class DialogoBajaCliente : Window
    {
        private readonly MVCliente _vm;
        private bool _escapeEnCurso = false;

        public DialogoBajaCliente(MVCliente vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private async void btnBajaCliente_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.ClienteSeleccionado == null)
            {
                MensajeInformacion.Mostrar("AVISO", "Selecciona un cliente.");
                return;
            }

            bool ok = await _vm.EliminarAsync(_vm.ClienteSeleccionado.IdCliente);

            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Cliente eliminado correctamente.");
                Close();
            }
            else
            {
                MensajeError.Mostrar("ERROR", "No se pudo eliminar el cliente.");
            }
        }

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
                    {
                        var main = Application.Current.Windows
                            .OfType<MainWindow>()
                            .FirstOrDefault();

                        main?.Activate();
                        Close();
                    }
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
    }
}
