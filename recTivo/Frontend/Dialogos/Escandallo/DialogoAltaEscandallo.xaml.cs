using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    public partial class DialogoAltaEscandallo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoAltaEscandallo(MVArticulo vm)
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
            if (DataContext is MVArticulo vm)
                vm.ComponentePadreSeleccionado = e.NewValue as recTivo.Backend.Modelos.ComponenteEscandallo;
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
