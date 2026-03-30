using di.proyecto.clase._2025.Frontend.Mensajes;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using recTivo.Backend.Repos;
using System.Windows;

namespace recTivo.Frontend.Dialogos
{
    public partial class Login : Window
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly IServiceProvider _serviceProvider;

        public Login(IServiceProvider serviceProvider, EmpleadoRepository empleadoRepository)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _empleadoRepository = empleadoRepository;
            Loaded += Window_Loaded;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MensajeAdvertencia.Mostrar("Advertencia de autenticación",
                    "Por favor, introduce usuario y clave.");
                return;
            }
            btnLogin.Content = "Validando...";

            try
            {
                var empleado = await _empleadoRepository.ValidarCredencialesAsync(usuario, password);

                if (empleado != null)
                {
                    if (Application.Current is App app)
                    {
                        app.EmpleadoActual = empleado;
                    }

                    var main = _serviceProvider.GetRequiredService<MainWindow>();

                    main.WindowState = WindowState.Maximized;
                    main.Show();

                    this.Close();
                }
                else
                {
                    MensajeError.Mostrar("Error de autenticación",
                        "Usuario o clave incorrectos.");

                    txtPassword.Clear();
                    txtUsuario.Focus();
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error",
                    $"Error al iniciar sesión: {ex.Message}");
            }
            finally
            {
                btnLogin.Content = "Entrar";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsuario.Focus();
        }
    }
}