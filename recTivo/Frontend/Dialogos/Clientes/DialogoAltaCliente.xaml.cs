using recTivo.Frontend.Mensajes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using recTivo.MVVM.Base;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace recTivo.Frontend.Dialogos.Clientes
{
    public partial class DialogoAltaCliente : Window
    {
        private readonly MVCliente _vm;

        public DialogoAltaCliente(MVCliente vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += (_, __) =>
            {
                _vm.LimpiarCampos();
                ForzarValidacion();
            };
        }

        private void OnErrorEvent(object sender, ValidationErrorEventArgs e)
        {
            if (DataContext is MVBase vm)
                vm.OnErrorEvent(sender, e);
        }

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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
            {
                var dialog = new ConfirmacionDialogo { Owner = this };
                bool? result = dialog.ShowDialog();
                if (result == true && dialog.Confirmado)
                {
                    var main = Application.Current.Windows
                        .OfType<MainWindow>()
                        .FirstOrDefault();
                    main?.Activate();
                    this.Close();
                }
                e.Handled = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVCliente vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private async void btnGuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            var ok = await _vm.GuardarAsync();
            if (ok)
            {
                MensajeInformacion.Mostrar("ÉXITO", "Cliente guardado correctamente.");
                this.Close();
            }
        }
    }
}