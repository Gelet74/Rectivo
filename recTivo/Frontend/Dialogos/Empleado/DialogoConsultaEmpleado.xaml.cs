using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Informes;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Empleado
{
    public partial class DialogoConsultaEmpleado : Window
    {
        private readonly MVEmpleado _vm;

        public DialogoConsultaEmpleado(MVEmpleado vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            _vm.LimpiarFiltros();
        }

        private void BtnExportarEmpleados_Click(object sender, RoutedEventArgs e)
        {
            var empleados = _vm.EmpleadosView != null
                ? _vm.EmpleadosView.Cast<recTivo.Backend.Modelos.Empleado>().ToList()
                : _vm.ListaEmpleados;
            var ruta = PdfService.GenerarListadoEmpleados(empleados);
            MensajeInformacion.Mostrar("PDF generado", $"Guardado en:\n{ruta}");
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

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }
    }
}