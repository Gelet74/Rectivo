using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Ordenes
{
    public partial class DialogoProcesarOrden : Window
    {
        private readonly MVOrden _vm;

        public DialogoProcesarOrden(MVOrden vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            rbSoloPT.IsChecked = true;

            // Fecha por defecto: hoy + 2 semanas
            var fechaDefecto = DateTime.Today.AddDays(14);
            dpFechaFin.SelectedDate = fechaDefecto;
            dpFechaFinPS.SelectedDate = fechaDefecto;
            _vm.FechaFin = fechaDefecto;
            _vm.FechaFinPS = fechaDefecto;

            Loaded += async (_, __) => await _vm.InicializarProcesoAsync();
        }

        // ── Pestaña PT: checkboxes ────────────────────────────────────────
        private void ChkPT_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is Articulo art)
                _vm.TogglePT(art, true);
        }

        private void ChkPT_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is Articulo art)
                _vm.TogglePT(art, false);
        }

        // ── Pestaña PT: radio buttons ─────────────────────────────────────
        private void RbSoloPT_Checked(object sender, RoutedEventArgs e)
        {
            _vm.IncluirPT = false;
            if (_vm.PreviewVisible) _ = _vm.CalcularPreviewAsync();
        }

        private void RbConPT_Checked(object sender, RoutedEventArgs e)
        {
            _vm.IncluirPT = true;
            if (_vm.PreviewVisible) _ = _vm.CalcularPreviewAsync();
        }

        // ── Pestaña PT: botones ───────────────────────────────────────────
        private async void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CalcularPreviewAsync();
        }

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is not App app || app.EmpleadoActual == null)
            {
                MensajeError.Mostrar("SESIÓN", "No hay ningún empleado con sesión iniciada.");
                return;
            }
            bool ok = await _vm.GenerarOrdenesAsync(app.EmpleadoActual);
            if (ok) Close();
        }

        // ── Pestaña PS: checkboxes ────────────────────────────────────────
        private void ChkPS_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is Articulo art)
                _vm.TogglePS(art, true);
        }

        private void ChkPS_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is Articulo art)
                _vm.TogglePS(art, false);
        }

        // ── Pestaña PS: botones ───────────────────────────────────────────
        private async void BtnCalcularPS_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CalcularPreviewPSAsync();
        }

        private async void BtnConfirmarPS_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is not App app || app.EmpleadoActual == null)
            {
                MensajeError.Mostrar("SESIÓN", "No hay ningún empleado con sesión iniciada.");
                return;
            }
            bool ok = await _vm.GenerarOrdenesPSAsync(app.EmpleadoActual);
            if (ok) Close();
        }

        // ── Cambio de pestaña: limpiar preview ───────────────────────────
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.PreviewVisible = false;
            _vm.PreviewPSVisible = false;
        }

        // ── ESC ───────────────────────────────────────────────────────────
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                var dialog = new ConfirmacionDialogo { Owner = this };
                if (dialog.ShowDialog() == true && dialog.Confirmado)
                    Close();
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }
    }
}