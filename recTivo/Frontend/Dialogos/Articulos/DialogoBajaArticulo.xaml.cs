using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM;
using recTivo.Frontend.Dialogos.VentanasInicio;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using di.proyecto.clase._2025.Frontend.Mensajes;

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoBajaArticulo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoBajaArticulo(MVArticulo vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) => await _vm.Inicializa();

        }

        private async void btnBajaArticulo_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _vm.BajaPorCodigoAsync();
            if (ok)
            {
                MensajeError.Mostrar("Error","Artículo dado de baja correctamente");
                this.Close();
            }
            else
            {
                MensajeError.Mostrar("Error", "No se encontró el artículo con ese código");
            }
        }


        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                var dialog = new ConfirmacionDialogo { Owner = this };
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.Confirmado)
                {
                    var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (main != null)
                        main.Activate();

                    this.Close();
                }

                e.Handled = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
