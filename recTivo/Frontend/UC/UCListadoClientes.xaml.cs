using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.VentanasInicio;
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
        private bool _escapeEnCurso = false;
        private readonly MVCliente _mvCliente;
        private readonly IServiceProvider _serviceProvider;
        private DialogoModificarCliente _dialogoModificarCliente;

        public event Action SolicitarCierre;
        public UCListadoClientes(MVCliente mvCliente,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mvCliente = mvCliente;
            _serviceProvider = serviceProvider;

            Loaded += async (_, __) =>
            {
                await _mvCliente.Inicializa();
                DataContext = _mvCliente;
                Focusable = true;
                Focus();
            };
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
                    var ownerWindow = Window.GetWindow(this);

                    var dialog = new ConfirmacionDialogo
                    {
                        Owner = ownerWindow
                    };

                    bool? result = dialog.ShowDialog();

                    if (result == true && dialog.Confirmado)
                        SolicitarCierre?.Invoke();
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

            _dialogoModificarCliente.panelDatos.Visibility = Visibility.Visible;

            _dialogoModificarCliente.ShowDialog();
        }
        }
    }


