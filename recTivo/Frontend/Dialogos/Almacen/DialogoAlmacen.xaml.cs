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
    /// Lógica de interacción para DialogoAlmacen.xaml
    /// </summary>
    public partial class DialogoAlmacen : Window
    {
        public DialogoAlmacen()
        {
            InitializeComponent();
        }

        private void EntradaAlmacen_Click(object sender, RoutedEventArgs e)
        { 
            this.Close();
            DialogoEntradaAlmacen dialogoEntradaAlmacen = new DialogoEntradaAlmacen();
            dialogoEntradaAlmacen.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogoEntradaAlmacen.ShowDialog();
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
        private void SalidaAlmacen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            DialogoSalidaAlmacen dialogoSalidaAlmacen = new DialogoSalidaAlmacen();
            dialogoSalidaAlmacen.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogoSalidaAlmacen.ShowDialog();

        }

    }
}
