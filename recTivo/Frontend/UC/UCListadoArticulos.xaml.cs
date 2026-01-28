using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.UC
{
    public partial class UCListadoArticulos : UserControl
    {
        private bool _escapeEnCurso = false;
        private readonly MVArticulo _mvArticulo;
        private readonly IServiceProvider _serviceProvider;
        private DialogoListarArticulo _dialogoListarArticulo;
        private DialogoModificarArticulo _dialogoModificarArticulo;

        // Evento para notificar al contenedor que debe cerrar esta vista
        public event Action SolicitarCierre;

        public UCListadoArticulos(MVArticulo mvArticulo,
                                  IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mvArticulo = mvArticulo;
            _serviceProvider = serviceProvider;

            Loaded += async (_, __) =>
            {
                await _mvArticulo.Inicializa();
                DataContext = _mvArticulo;

                // Necesario para que ESC funcione
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

        private async void modificarArticulo_Click(object sender, RoutedEventArgs e)
        {
            if (_mvArticulo.ArticuloSeleccionado == null)
            {
                MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo primero.");
                return;
            }

            _dialogoModificarArticulo = _serviceProvider.GetRequiredService<DialogoModificarArticulo>();

            // Pasar el mismo ViewModel
            _dialogoModificarArticulo.DataContext = _mvArticulo;

            // Establecer el código seleccionado
            _mvArticulo.CodigoSeleccionado = _mvArticulo.ArticuloSeleccionado.Codigo;

            // Cargar datos del artículo
            await _mvArticulo.CargarArticuloSeleccionadoAsync();

            // Mostrar panel de datos directamente
            _dialogoModificarArticulo.panelDatos.Visibility = Visibility.Visible;

            _dialogoModificarArticulo.ShowDialog();
        }


    }
}
