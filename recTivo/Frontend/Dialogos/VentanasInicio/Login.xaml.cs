using MahApps.Metro.Controls;
using recTivo.Backend.Repos;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using di.proyecto.clase._2025.Frontend.Mensajes;

namespace recTivo.Frontend.Dialogos
{
    public partial class Login : MetroWindow
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly IServiceProvider _serviceProvider;

        // Constructor con DI
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
                MensajeAdvertencia.Mostrar("Advertencia de autenticación", "Por favor, introduce usuario y clave.");
            }
            else
            {
                var empleado = await _empleadoRepository.ValidarCredencialesAsync(usuario, password);

                if (empleado != null)
                {
                    // ⭐ GUARDAR EL EMPLEADO EN App.EmpleadoActual
                    if (Application.Current is App app)
                    {
                        app.EmpleadoActual = empleado;
                    }

                    // Resolvemos MainWindow desde el contenedor
                    var main = _serviceProvider.GetService(typeof(MainWindow)) as MainWindow;
                    main.WindowState = WindowState.Maximized;
                    main.Show();
                    this.Close();
                }
                else
                {
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