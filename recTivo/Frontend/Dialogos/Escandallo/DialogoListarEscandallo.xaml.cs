using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
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
            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private async void cmbCodigo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No hacer nada aquí, solo esperar al botón
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            var codigo = _vm.ArticuloSeleccionado?.Codigo; // ← CAMBIO: ArticuloSeleccionado

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MensajeError.Mostrar("ESCANDALLO", "Selecciona un código de artículo.");
                return;
            }

            await _vm.CargarEscandalloAsync(codigo); // ← CAMBIO: CargarEscandalloAsync
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm.ComponentePadreSeleccionado = e.NewValue as ComponenteEscandallo;
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