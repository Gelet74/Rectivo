using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using recTivo.MVVM.Base;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Empleado
{
    public partial class DialogoAltaEmpleado : Window
    {
        private readonly MVEmpleado _vm;

        public DialogoAltaEmpleado(MVEmpleado vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private void OnErrorEvent(object sender, ValidationErrorEventArgs e)
        { 
            if (DataContext is MVBase vm) 
                vm.OnErrorEvent(sender, e); 
        }

        private async void btnAltaEmpleado_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _vm.GuardarAsync();
            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Empleado guardado correctamente");
                this.Close();
            }
        }

        private bool _escapeEnCurso = false;
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
                        this.Close();
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
