using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace recTivo.Frontend.Dialogos
{
    public partial class Inicio : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public Inicio(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            Loaded += Inicio_Loaded;
        }

        private async void Inicio_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000);

            var login = _serviceProvider.GetService<Login>();
            if (login != null)
            {
                login.Show();
            }
            else
            {
                MessageBox.Show("Login no está registrado en el contenedor.");
            }

            this.Close();
        }

    }
}
