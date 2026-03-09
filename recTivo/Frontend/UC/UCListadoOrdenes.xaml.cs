using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Informes;
using recTivo.MVVM;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.UC
{
    public partial class UCListadoOrdenes : UserControl
    {
        private readonly MVOrden _vm;
        private bool _escapeEnCurso = false;

        public event Action? SolicitarCierre;

        public UCListadoOrdenes(MVOrden vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) =>
            {
                await _vm.InicializarListadoAsync();
                Focusable = true;
                Focus();
            };
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
            => _vm.LimpiarFiltros();

        private async void BtnRecargar_Click(object sender, RoutedEventArgs e)
            => await _vm.CargarOrdenesAsync();

        // ── PDF: lista de órdenes filtradas ──────────────────────────────
        private void BtnExportarOrdenes_Click(object sender, RoutedEventArgs e)
        {
            if (!_vm.OrdenesFiltradas.Any())
            {
                MensajeInformacion.Mostrar("ÓRDENES", "No hay órdenes para exportar.");
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Guardar informe de órdenes",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Ordenes_{DateTime.Now:yyyyMMdd}"
            };

            if (dlg.ShowDialog() != true) return;

            string ruta = PdfService.GenerarListadoOrdenes(_vm.OrdenesFiltradas, dlg.FileName);
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }

        // ── PDF: detalle de la orden seleccionada con sus fases ──────────
        private void BtnDetalleOrden_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.OrdenSeleccionada == null)
            {
                MensajeInformacion.Mostrar("ÓRDENES", "Selecciona una orden primero.");
                return;
            }

            string ruta = PdfService.GenerarDetalleOrden(
                _vm.OrdenSeleccionada,
                _vm.FasesOrden);

            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }

        // ── Escape ───────────────────────────────────────────────────────
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_escapeEnCurso) { e.Handled = true; return; }
                _escapeEnCurso = true;
                e.Handled = true;
                try
                {
                    var dialog = new ConfirmacionDialogo { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true && dialog.Confirmado)
                        SolicitarCierre?.Invoke();
                }
                finally { _escapeEnCurso = false; }
            }
            else base.OnPreviewKeyDown(e);
        }
    }
}