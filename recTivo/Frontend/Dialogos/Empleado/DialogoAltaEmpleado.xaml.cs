using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using recTivo.MVVM.Base;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace recTivo.Frontend.Dialogos.Empleado
{
    public partial class DialogoAltaEmpleado : Window
    {
        private readonly MVEmpleado _vm;

        public DialogoAltaEmpleado(MVEmpleado vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, __) =>
            {
                await _vm.Inicializa();
                _vm.LimpiarCampos();
                ForzarValidacion();
            };
        }

        private void OnErrorEvent(object sender, ValidationErrorEventArgs e)
        {
            if (DataContext is MVBase vm)
                vm.OnErrorEvent(sender, e);
        }

        // Fuerza que WPF evalúe todos los TextBox con ValidatesOnDataErrors
        // para que aparezcan en rojo con su mensaje desde el inicio
        private void ForzarValidacion()
        {
            foreach (var tb in FindVisualChildren<TextBox>(this))
                tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t) yield return t;
                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        private async void btnAltaEmpleado_Click(object sender, RoutedEventArgs e)
        {
            bool ok = await _vm.GuardarAsync();
            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Empleado guardado correctamente");
                this.Close();
            }
        }

        private bool _escapeEnCurso = false;
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_escapeEnCurso)
                {
                    e.Handled = true;
                    return;
                }

                _escapeEnCurso = true;
                e.Handled = true;

                try
                {
                    var dialog = new ConfirmacionDialogo { Owner = this };
                    bool? result = dialog.ShowDialog();

                    if (result == true && dialog.Confirmado)
                    {
                        this.Close();
                    }
                }
                finally
                {
                    _escapeEnCurso = false;
                }
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVEmpleado vm)
            {
                vm.Empleado.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}