using recTivo.Frontend.Dialogos.VentanasInicio;
using System.Windows;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos.Ventas
{
    /// <summary>
    /// Lógica de interacción para DialogoCrearPedido.xaml
    /// </summary>
    public partial class DialogoCrearPedido : Window
    {
        private bool _escapeEnCurso = false;
        public DialogoCrearPedido()
        {
            InitializeComponent();
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
