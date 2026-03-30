using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Informes;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.UC
{
    /// <summary>
    /// Lógica de interacción para UCListadoClientes.xaml
    /// </summary>
    public partial class UCListadoClientes : UserControl
    {
        private readonly MVCliente _mvCliente;
        private readonly IServiceProvider _serviceProvider;
        private DialogoModificarCliente? _dialogoModificarCliente;

        public event Action? SolicitarCierre;
        public UCListadoClientes(MVCliente mvCliente,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mvCliente = mvCliente;
            _serviceProvider = serviceProvider;

            Loaded += async (_, __) =>
            {
                DataContext = _mvCliente;
                await _mvCliente.Inicializa();

                Focusable = true;
                Focus();
            };
        }

        private void BtnExportarClientes_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MVCliente)DataContext;
            var clientes = vm.ClientesView?.Cast<Cliente>() ?? vm.ListaClientes;
            var ruta = PdfService.GenerarListadoClientes(clientes);
            MensajeInformacion.Mostrar("PDF generado", $"Guardado en:\n{ruta}");
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVCliente vm)
                vm.LimpiarFiltros();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                SolicitarCierre?.Invoke();
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private async void modificarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (_mvCliente.ClienteSeleccionado == null)
            {
                MensajeError.Mostrar("ERROR", "Debes seleccionar un cliente primero.");
                return;
            }
            _dialogoModificarCliente = _serviceProvider.GetRequiredService<DialogoModificarCliente>();

            _dialogoModificarCliente.DataContext = _mvCliente;

            await _mvCliente.CargarClienteSeleccionadoAsync();

            _dialogoModificarCliente.panelEdicion.Visibility = Visibility.Visible;

            _dialogoModificarCliente.ShowDialog();
        }
    }
}


