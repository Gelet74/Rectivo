using System.Linq;
using System.Windows;
using System.Windows.Input;
using recTivo.Frontend.Dialogos.VentanasInicio;

namespace recTivo.Frontend.Dialogos
{
    public class BaseDialogo : Window
    {
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

                    if (main != null)
                        main.Activate();

                    this.Close();
                }

                e.Handled = true;
            }
        }
    }
}
