
using recTivo.Frontend.Dialogos.VentanasInicio;
using System.Windows;
using System.Windows.Input;


namespace recTivo.Frontend.Dialogos.Articulos
{ 
    /// <summary>
    /// Lógica de interacción para DialogoAltaArticulo.xaml
    /// </summary>




        public partial class DialogoAltaArticulo : Window
{
         public DialogoAltaArticulo()
    {
        InitializeComponent();
    }

        private void btnAltaArticulo_Click(object sender, RoutedEventArgs e)
        {

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