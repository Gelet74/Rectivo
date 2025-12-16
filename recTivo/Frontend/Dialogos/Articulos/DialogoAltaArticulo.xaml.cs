using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoAltaArticulo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoAltaArticulo()
        {
            InitializeComponent();

            // 👇 Instanciamos manualmente el contexto y el repositorio
            var context = new RectivoContext();
            var repo = new ArticuloRepository(context);

            _vm = new MVArticulo(repo);
            DataContext = _vm;
        }

        private async void btnAltaArticulo_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _vm.GuardarAsync();
            if (ok)
            {
                MensajeInformacion.Mostrar("Éxito","Artículo dado de alta correctamente");
                this.Close();
            }
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
