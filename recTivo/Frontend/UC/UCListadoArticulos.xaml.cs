using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Informes;
using recTivo.MVVM;
using System.Diagnostics;
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

        public event Action SolicitarCierre;

        public UCListadoArticulos(MVArticulo mvArticulo, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _mvArticulo = mvArticulo;
            _serviceProvider = serviceProvider;

            Loaded += async (_, __) =>
            {
                await _mvArticulo.Inicializa();
                DataContext = _mvArticulo;
                Focusable = true;
                Focus();
            };
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            _mvArticulo.LimpiarFiltros();
        }

        // ── PDF: listado de artículos ────────────────────────────────────
        private void BtnExportarArticulos_Click(object sender, RoutedEventArgs e)
        {
            if (_mvArticulo.ListaArticulos == null || !_mvArticulo.ListaArticulos.Any())
            {
                MensajeInformacion.Mostrar("ARTÍCULOS", "No hay artículos para exportar.");
                return;
            }

            string ruta = PdfService.GenerarListadoArticulos(_mvArticulo.ListaArticulos);
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }

        private void modificarArticulo_Click(object sender, RoutedEventArgs e)
        {
            if (_mvArticulo.ArticuloSeleccionado == null)
            {
                MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo primero.");
                return;
            }

            var vmModificar = _serviceProvider.GetRequiredService<MVArticulo>();
            vmModificar.ArticuloSeleccionado = _mvArticulo.ArticuloSeleccionado;

            var dialogo = new DialogoModificarArticulo(vmModificar)
            {
                Owner = Window.GetWindow(this)
            };

            dialogo.ShowDialog();
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
                    var dialog = new ConfirmacionDialogo { Owner = ownerWindow };
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
    }
}