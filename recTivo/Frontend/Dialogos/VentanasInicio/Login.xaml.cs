using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using System.Windows;
using Microsoft.EntityFrameworkCore.SqlServer;
using di.proyecto.clase._2025.Frontend.Mensajes;

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
            var logger = loggerFactory.CreateLogger<GenericRepository<recTivo.Backend.Modelos.Empleado>>();

            // Crear repositorio
            _empleadoRepository = new EmpleadoRepository(context, logger);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Primer caso: campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MensajeAdvertencia.Mostrar("Advertencia de autenticación", "Por favor, introduce usuario y clave.");
            }
            else
            {
                // Segundo caso: comprobamos credenciales
                var empleado = await _empleadoRepository.ValidarCredencialesAsync(usuario, password);

                if (empleado != null)
                {
                    var main = new MainWindow
                    {
                        WindowState = WindowState.Maximized
                    };
                    main.Show();
                    this.Close();
                }
                else
                {
                    // Tercer caso: credenciales incorrectas
                    MensajeError.Mostrar("Error de autenticación", "Usuario o clave incorrectos.");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsuario.Focus();
        }
    }
}
