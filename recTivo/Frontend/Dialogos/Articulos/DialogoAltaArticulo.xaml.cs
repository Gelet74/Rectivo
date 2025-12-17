using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Articulos
{
    /// <summary>
    /// Interaction logic for DialogoArticulo.xaml
    /// </summary>
    public partial class DialogoAltaArticulo : Window
    {
        private MVArticulo _mvArticulo;
        public DialogoAltaArticulo(MVArticulo mvArticulo)
        {
            InitializeComponent();

            _mvArticulo = mvArticulo;
        }

        private async void btnAltaArticulo_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _mvArticulo.GuardarAsync();
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
