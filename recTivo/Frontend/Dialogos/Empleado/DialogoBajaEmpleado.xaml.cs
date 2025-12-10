using recTivo.Frontend.Dialogos.VentanasInicio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace recTivo.Frontend.Dialogos.Empleado
{
    /// <summary>
    /// Lógica de interacción para DialogoBajaEmpleado.xaml
    /// </summary>
    public partial class DialogoBajaEmpleado : Window
    {
        public DialogoBajaEmpleado()
        {
            InitializeComponent();
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                var dialog = new ConfirmacionDialogo
                {
                    Owner = this
                };

                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Confirmado)
                {
                    var main = Application.Current.Windows
                        .OfType<MainWindow>()
                        .FirstOrDefault();

                    if (main != null)
                    {
                        main.Activate();
                    }

                    this.Close();
                }

                e.Handled = true;
            }
        }
    }
}
