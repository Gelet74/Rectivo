using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Informes;
using recTivo.MVVM;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoListarEscandallo : Window
    {
        private readonly MVEscandallo _vm;
        private bool _escapeEnCurso = false;

        public DialogoListarEscandallo(MVEscandallo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            var codigo = _vm.CodigoSeleccionado;
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MensajeInformacion.Mostrar("AVISO", "Selecciona un código de artículo.");
                return;
            }

            bool tieneEscandallo = await _vm.TieneEscandallo(codigo);
            if (!tieneEscandallo)
            {
                MensajeError.Mostrar("LISTAR ESCANDALLO",
                    $"El artículo '{codigo}' no tiene ningún escandallo creado.");
                return;
            }

            await _vm.CargarEscandallo(codigo);
        }

        // ── PDF: escandallo cargado actualmente ──────────────────────────
        private void BtnExportarEscandallo_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.ArticuloFinal == null)
            {
                MensajeInformacion.Mostrar("ESCANDALLO", "Carga un escandallo primero.");
                return;
            }

            // Construimos el objeto Escandallo a partir del ArticuloFinal cargado
            var escandallo = new recTivo.Backend.Modelos.Escandallo
            {
                CodigoProducto = _vm.ArticuloFinal.Codigo,
                Descrip = _vm.ArticuloFinal.descrip ?? "",
                Descrip2 = _vm.ArticuloFinal.descrip2
            };

            // Aplanar el árbol jerárquico de componentes en lista plana
            var componentes = AplanarComponentes(_vm.EscandalloActual);

            string ruta = PdfService.GenerarEscandallo(escandallo, componentes);
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }

        // Helper: recorre el árbol de ComponenteEscandallo recursivamente
        private static IEnumerable<ComponenteEscandallo> AplanarComponentes(
            IEnumerable<ComponenteEscandallo> nodos)
        {
            foreach (var nodo in nodos)
            {
                yield return nodo;
                if (nodo.Hijos != null)
                    foreach (var hijo in AplanarComponentes(nodo.Hijos))
                        yield return hijo;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm.ComponentePadreSeleccionado = e.NewValue as ComponenteEscandallo;
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
                    var dialog = new ConfirmacionDialogo { Owner = this };
                    bool? result = dialog.ShowDialog();

                    if (result == true && dialog.Confirmado)
                        this.Close();
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