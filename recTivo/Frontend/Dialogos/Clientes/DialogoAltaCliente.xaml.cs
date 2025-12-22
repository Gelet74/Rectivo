using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Clientes
{
    public partial class DialogoAltaCliente : Window
    {
        private readonly MVCliente _vm;

        public DialogoAltaCliente(MVCliente vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += (_, __) => _vm.LimpiarCampos();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                var dialog = new ConfirmacionDialogo { Owner = this };
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Confirmado)
                {
                    var main = Application.Current.Windows
                        .OfType<MainWindow>()
                        .FirstOrDefault();

                    main?.Activate();
                    this.Close();
                }

                e.Handled = true;
            }
        }

        private async void btnGuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            var ok = await _vm.GuardarAsync();

            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Cliente guardado correctamente.");
                this.Close();
            }
        }
    }
}
