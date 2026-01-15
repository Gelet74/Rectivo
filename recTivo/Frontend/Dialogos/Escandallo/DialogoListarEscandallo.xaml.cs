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
        private readonly MVArticulo _vm;
        public DialogoListarEscandallo(MVArticulo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private void txtCodigo_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == null)
                return;

            int caret = tb.CaretIndex;

            string mayus = tb.Text.ToUpper();

            if (tb.Text != mayus)
            {
                tb.Text = mayus;
                tb.CaretIndex = caret;
            }
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVArticulo vm)
                await vm.CargarEscandalloAsync(vm.CodigoSeleccionado);
        }


        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MVArticulo vm)
                vm.ComponenteSeleccionado = e.NewValue as ComponenteEscandallo;
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
                    {
                        this.Close();
                    }
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
