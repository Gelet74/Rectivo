using recTivo.Frontend.Dialogos.Empleado;
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

namespace recTivo.Frontend.Dialogos
{
    /// <summary>
    /// Lógica de interacción para DialogoEmpleado.xaml
    /// </summary>
    public partial class DialogoEmpleado : Window
    {
        public DialogoEmpleado()
        {
            InitializeComponent();
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == System.Windows.Input.Key.Escape)
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
        }
        private void alta_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            DialogoAltaEmpleado dialogoAltaEmpleado = new DialogoAltaEmpleado();
            dialogoAltaEmpleado.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogoAltaEmpleado.ShowDialog();
        }

        private void baja_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            DialogoBajaEmpleado dialogoBajaEmpleado = new DialogoBajaEmpleado();
            dialogoBajaEmpleado.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogoBajaEmpleado.ShowDialog();
        }

        private void modificar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void consulta_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
