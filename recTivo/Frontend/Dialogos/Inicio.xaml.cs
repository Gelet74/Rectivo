using MahApps.Metro.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace recTivo.Frontend.Dialogos
{
    /// <summary>
    /// Lógica de interacción para Inicio.xaml
    /// </summary>
    public partial class Inicio : MetroWindow
    {
        public Inicio()
        {
            InitializeComponent();
            Loaded += Inicio_Loaded;
        }

        private async void Inicio_Loaded(object sender, RoutedEventArgs e)
        {
            // Espera 3 segundos con animación
            await Task.Delay(3000);

            // Abre la ventana de login
            var login = new Login();
            login.Show();

            // Cierra el splash
            this.Close();
        }
    }
}
