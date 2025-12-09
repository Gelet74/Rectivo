using MahApps.Metro.Controls;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Empleado;
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

        private void almacen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (almacen.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Entradas almacén":
                    new DialogoEntradaAlmacen { Owner = this }.ShowDialog();
                    break;

                case "Salidas almacén":
                    new DialogoSalidaAlmacen { Owner = this }.ShowDialog();
                    break;
            }

            almacen.SelectedItem = null;
        }

        private void articulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (articulos.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Dar de alta":
                    new DialogoAltaArticulo { Owner = this }.ShowDialog();
                    break;

                case "Dar de baja":
                    new DialogoBajaArticulo { Owner = this }.ShowDialog();
                    break;
                case "Modificar":
                    new DialogoModificarArticulo { Owner = this }.ShowDialog();
                    break;

                case "Listar artículos":
                    new DialogoListarArticulo { Owner = this }.ShowDialog();
                    break;
            }



            articulos.SelectedItem = null;
        }


        private void empleados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (empleados.SelectedItem is not ListViewItem item)
                return;

            string opcion = item.Content.ToString();

            switch (opcion)
            {
                case "Dar de alta":
                    new DialogoAltaEmpleado { Owner = this }.ShowDialog();
                    break;

                case "Dar de baja":
                    new DialogoBajaEmpleado { Owner = this }.ShowDialog();
                    break;
                case "Modificar":
                    new DialogoModificarEmpleado { Owner = this }.ShowDialog();
                    break;

                case "Listar empleados":
                    new DialogoConsultaEmpleado { Owner = this }.ShowDialog();
                    break;

            }

            empleados.SelectedItem = null;
        }




        private void salir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void articulos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}