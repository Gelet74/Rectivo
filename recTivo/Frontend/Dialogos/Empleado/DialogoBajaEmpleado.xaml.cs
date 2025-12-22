using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Empleado
{
    public partial class DialogoBajaEmpleado : Window
    {
        private readonly MVEmpleado _vm;
        private bool _escapeEnCurso = false;

        public DialogoBajaEmpleado(MVEmpleado vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            // ✅ Cargar empleados al abrir la ventana
            Loaded += async (_, __) => await _vm.Inicializa();
        }

        // ✅ Botón Dar de Baja
        private async void btnBajaEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.EmpleadoSeleccionado == null)
            {
                MensajeInformacion.Mostrar("AVISO", "Selecciona un empleado.");
                return;
            }

            bool ok = await _vm.EliminarAsync(_vm.EmpleadoSeleccionado.Id);


            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Empleado eliminado correctamente.");
                Close();
            }
            else
            {
                MensajeError.Mostrar("ERROR", "No se pudo eliminar el empleado.");
            }
        }

        // ✅ Manejo de ESC con confirmación
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
