using MahApps.Metro.Controls;
using recTivo.Frontend.Dialogos;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace recTivo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Articulos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Clientes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Almacen_Click(object sender, RoutedEventArgs e)
        {
            DialogoAlmacen dialogoAlmacen = new DialogoAlmacen();
            dialogoAlmacen.ShowDialog();
        }

        private void Ordenes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void empleados_Click(object sender, RoutedEventArgs e)
        {
            DialogoEmpleado dialogoEmpleado = new DialogoEmpleado();
            dialogoEmpleado.ShowDialog();
        }

        private void ventas_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}