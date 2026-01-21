using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoAltaEscandallo : Window
    {
        private readonly MVEscandallo _vm;

        public DialogoAltaEscandallo(MVEscandallo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        // ================================
        //   MANEJO DE ESCAPE
        // ================================
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

        // ================================
        //   AÑADIR COMPONENTE RAÍZ
        // ================================
        private void BtnAñadir_Click(object sender, RoutedEventArgs e)
        {
            _vm.AñadirComponente();
        }

        // ================================
        //   CARGAR ESCANDALLO
        // ================================
        private async void BtnCargarEscandallo_Click(object sender, RoutedEventArgs e)
        {
            var articulo = _vm.ArticuloFinal;
            if (articulo == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Selecciona un artículo válido.");
                return;
            }

            await _vm.CargarEscandallo(articulo.Codigo);
        }


   
        // ================================
        //   AÑADIR SUBCOMPONENTE
        // ================================
        private void BtnAñadirHijo_Click(object sender, RoutedEventArgs e)
        {
            _vm.AñadirSubcomponente();
        }

        // ================================
        //   SELECCIÓN EN TREEVIEW
        // ================================
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var seleccionado = e.NewValue as ComponenteEscandallo;
            _vm.ComponentePadreSeleccionado = seleccionado;
            _vm.ComponenteSeleccionado = seleccionado;
        }



        // ================================
        //   GUARDAR ESCANDALLO
        // ================================
        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            await _vm.GuardarEscandallo();
        }
    }
}
