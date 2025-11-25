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
    /// Lógica de interacción para DialogoSalidaAlmacen.xaml
    /// </summary>
    public partial class DialogoSalidaAlmacen : Window
    {
        public DialogoSalidaAlmacen()
        {
            InitializeComponent();
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                // Recuperar la ventana principal que ya está abierta
                var main = Application.Current.Windows
                    .OfType<MainWindow>()
                    .FirstOrDefault();

                if (main != null)
                {
                    // Traerla al frente
                    main.Activate();
                }

                // Cerrar el diálogo actual
                this.Close();
            }
        }


    }
}
