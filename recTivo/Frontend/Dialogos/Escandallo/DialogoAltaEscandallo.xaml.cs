using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Escandallo
{
    /// <summary>
    /// Lógica de interacción para DialogoAltaEscandallo.xaml
    /// </summary>
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

        private void BtnAñadir_Click(object sender, RoutedEventArgs e)
        {
            _vm.AñadirComponente();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            await _vm.GuardarEscandallo();
        }

       
    }
}
