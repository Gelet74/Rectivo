using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using System.Windows;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace recTivo.Frontend.Dialogos
{
    public partial class Login : MetroWindow
    {
        private readonly EmpleadoRepository _empleadoRepository;

        public Login()
        {
            InitializeComponent();

            // Configuración del DbContext directamente aquí
            var optionsBuilder = new DbContextOptionsBuilder<RectivoContext>();
            optionsBuilder.UseMySQL("server=localhost;database=recTivoDB;user=root;password=tuPassword;");


            var context = new RectivoContext(optionsBuilder.Options);

            // Logger opcional
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<GenericRepository<Empleado>>();

            // Crear repositorio
            _empleadoRepository = new EmpleadoRepository(context, logger);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();

            var empleado = await _empleadoRepository.ValidarCredencialesAsync(usuario, password);

            if (empleado != null)
            {
                var main = new MainWindow();
                main.WindowState = WindowState.Maximized;
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsuario.Focus();
        }
    }
}
