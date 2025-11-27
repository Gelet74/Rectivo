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
    /// Lógica de interacción para DialogoAltaEmpleado.xaml
    /// </summary>
    public partial class DialogoAltaEmpleado : Window
    {
        public DialogoAltaEmpleado()
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
    }
}
