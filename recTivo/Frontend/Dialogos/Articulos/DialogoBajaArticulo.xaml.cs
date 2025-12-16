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

namespace recTivo.Frontend.Dialogos.Articulos
{
    public partial class DialogoBajaArticulo : Window
    {
        private readonly MVArticulo _vm;

        public DialogoBajaArticulo()
        {
            InitializeComponent();

            // 👇 Instanciamos manualmente el contexto y el repositorio
            var context = new RectivoContext();
            var repo = new ArticuloRepository(context);

            _vm = new MVArticulo(repo);
            DataContext = _vm;
        }

        private async void btnBajaArticulo_Click(object sender, RoutedEventArgs e)
        {
            string codigo = txtCodigoArticulo.Text.Trim();

            if (!string.IsNullOrEmpty(codigo))
            {
                bool ok = await _vm.BajaPorCodigoAsync(codigo);
                if (ok)
                {
                    MessageBox.Show("Artículo dado de baja correctamente");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se encontró el artículo con ese código");
                }
            }
            else
            {
                MessageBox.Show("Introduce un código válido");
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
