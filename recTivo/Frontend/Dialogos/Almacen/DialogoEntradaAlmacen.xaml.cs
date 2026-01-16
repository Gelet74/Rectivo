using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos
{
    public partial class DialogoEntradaAlmacen : Window
    {
        private bool _escapeEnCurso = false;

        public DialogoEntradaAlmacen(MVArticulo vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += async (_, _) => await vm.Inicializa();
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

        private async void btnAnadirAlmacen_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVArticulo vm)
                await vm.AñadirAlmacen();
        }
    }
}
