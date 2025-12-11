using recTivo.Frontend.Dialogos.VentanasInicio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace recTivo.Frontend.Dialogos.Articulos
{
    using global::recTivo.Backend.Modelos;

    /// <summary>
    /// Lógica de interacción para DialogoAltaArticulo.xaml
    /// </summary>
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    namespace recTivo.Frontend.Dialogos.Articulos
    {
        public partial class DialogoAltaArticulo : Window
        {
            private readonly RectivoContext _context;
            private bool _escapeEnCurso = false;

            public DialogoAltaArticulo(RectivoContext context)
            {
                InitializeComponent();
                _context = context;
            }

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
                            var main = Application.Current.Windows
                                .OfType<MainWindow>()
                                .FirstOrDefault();

                            if (main != null)
                                main.Activate();

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

            private void btnAltaArticulo_Click(object sender, RoutedEventArgs e)
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtDescrip1.Text))
                {
                    MessageBox.Show("Código y Descripción 1 son obligatorios.", "Alta artículo",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal? pvp = null;
                if (decimal.TryParse(txtPvp.Text, out decimal parsed))
                    pvp = parsed;

                var nuevoArticulo = new Articulo
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Descripcion1 = txtDescrip1.Text.Trim(),
                    Descripcion2 = txtDescrip2.Text.Trim(),
                    Pvp = pvp
                };

                _context.Articulos.Add(nuevoArticulo);
                _context.SaveChanges();

                MessageBox.Show("Artículo dado de alta correctamente.",
                                "Alta artículo", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
        }
    }

