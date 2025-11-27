using System.Windows;
using System.Windows.Input;
using recTivo.Backend.Modelos;


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

        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
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
       
        private void DialAltaEmpleado_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new RectivoContext();
                var roles = db.Rols.ToList();
                cmbRol.ItemsSource = roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando roles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnAltaEmpleado_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtDni.Text) ||
                    string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Por favor, completa todos los campos son obligatorios.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (txtDni.Text.Length < 9)
                {
                    MessageBox.Show("El DNI debe tener al menos 9 caracteres.", "DNI inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                              
                var empleado = new recTivo.Backend.Modelos.Empleado
                {
                    Apellidos = txtApellidos.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Dni = txtDni.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text, 
                    IdRol = (int?)cmbRol.SelectedValue,
                    Estado = "activo"
                };

                var db = new RectivoContext();
                db.Empleados.Add(empleado);
                db.SaveChanges();

                MessageBox.Show("Empleado dado de alta correctamente.", "Confirmación", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al dar de alta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
