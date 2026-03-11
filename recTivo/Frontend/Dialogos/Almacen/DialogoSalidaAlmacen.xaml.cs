using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos
{
    public partial class DialogoSalidaAlmacen : Window
    {
        private bool _escapeEnCurso = false;

        public DialogoSalidaAlmacen(MVAlmacen vm)
        {
            InitializeComponent();
            DataContext = vm;

            // Inicializa los artículos desde MVArticulo
            Loaded += async (_, __) => await vm.MVArticulo.Inicializa();
        }

        private void TextBox_SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == ',' || c == '.');
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

        private async void btnRestarAlmacen_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVAlmacen vm)
                await vm.SalidaAlmacen();
        }
    }
}
