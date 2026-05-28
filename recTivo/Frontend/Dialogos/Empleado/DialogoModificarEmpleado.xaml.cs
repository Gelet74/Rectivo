using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Empleado
{
    public partial class DialogoModificarEmpleado : Window
    {
        private readonly MVEmpleado _vm;

        public DialogoModificarEmpleado(MVEmpleado vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) =>
            {
                _vm.LimpiarCampos();
                await _vm.Inicializa();
            };
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

        private async void btnCargarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.EmpleadoSeleccionado == null)
            {
                MensajeError.Mostrar("MODIFICAR EMPLEADO", "Debes seleccionar un empleado.");
                return;
            }

            var ok = await _vm.CargarEmpleadoSeleccionadoAsync();

            if (!ok)
            {
                MensajeError.Mostrar("MODIFICAR EMPLEADO", "No se ha encontrado el empleado.");
                panelDatos.Visibility = Visibility.Collapsed;
                return;
            }

            panelDatos.Visibility = Visibility.Visible;
        }

        private async void btnModificarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            var ok = await _vm.ModificarEmpleadoAsync();

            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Empleado modificado correctamente.");
                this.Close();
            }
        }
    }
}
