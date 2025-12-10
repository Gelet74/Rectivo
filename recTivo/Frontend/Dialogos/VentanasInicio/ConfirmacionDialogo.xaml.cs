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

namespace recTivo.Frontend.Dialogos.VentanasInicio
{
    /// <summary>
    /// Lógica de interacción para ConfirmacionDialogo.xaml
    /// </summary>
    public partial class ConfirmacionDialogo : Window
    {
        public bool Confirmado { get; private set; }
        public ConfirmacionDialogo()
        {
            InitializeComponent();
        }

        private void BtnSi_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.DialogResult = false;
            this.Close();
        }


    }
}
