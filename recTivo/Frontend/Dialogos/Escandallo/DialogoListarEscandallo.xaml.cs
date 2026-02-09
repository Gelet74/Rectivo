using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoListarEscandallo : Window
    {
        private readonly MVEscandallo _vm;

        public DialogoListarEscandallo(MVEscandallo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) =>
            {
                await _vm.Inicializa();

                // ✅ TEST TEMPORAL
                await _vm.TestBuscarEscandallo();
            };
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.Inicializa();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR AL CARGAR",
                    $"No se pudo inicializar el diálogo:\n{ex.Message}\n\nDetalles: {ex.StackTrace}");

                // Log para debugging
                System.Diagnostics.Debug.WriteLine($"ERROR en Loaded: {ex}");

                // Cerrar el diálogo si falla la inicialización
                this.Close();
            }
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MensajeError.Mostrar("ERROR CRÍTICO",
                $"Ha ocurrido un error inesperado:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}");

            System.Diagnostics.Debug.WriteLine($"ERROR NO MANEJADO: {e.Exception}");

            e.Handled = true; // Evitar que cierre la aplicación
            this.Close();
        }

        private async void cmbCodigo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No hacer nada aquí, solo esperar al botón
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var codigo = _vm.CodigoSeleccionado;
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MensajeInformacion.Mostrar("AVISO", "Selecciona un código de artículo.");
                    return;
                }

                await _vm.CargarEscandallo(codigo);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR AL CARGAR ESCANDALLO",
                    $"No se pudo cargar el escandallo:\n{ex.Message}");

                System.Diagnostics.Debug.WriteLine($"ERROR en BtnCargar_Click: {ex}");
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                _vm.ComponentePadreSeleccionado = e.NewValue as ComponenteEscandallo;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR",
                    $"Error al seleccionar componente:\n{ex.Message}");

                System.Diagnostics.Debug.WriteLine($"ERROR en TreeView_SelectedItemChanged: {ex}");
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
                        this.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR en OnPreviewKeyDown: {ex}");
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