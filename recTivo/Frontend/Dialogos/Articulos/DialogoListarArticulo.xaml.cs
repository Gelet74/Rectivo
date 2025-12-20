using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoListarArticulo : Window
    {
        private MVArticulo _vm;

        public DialogoListarArticulo(MVArticulo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                var dialog = new ConfirmacionDialogo { Owner = this };
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Confirmado)
                {
                    var main = Application.Current.Windows
                        .OfType<MainWindow>()
                        .FirstOrDefault();

                    main?.Activate();
                    this.Close();
                }

                e.Handled = true;
            }
        }
    }
}
