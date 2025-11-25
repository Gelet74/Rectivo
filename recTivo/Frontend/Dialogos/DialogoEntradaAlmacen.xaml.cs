using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace recTivo.Frontend.Dialogos
{
    /// <summary>
    /// Lógica de interacción para DialogoEntradaAlmacen.xaml
    /// </summary>
    public partial class DialogoEntradaAlmacen : Window
    {
        private RectivoContext _context;
        private List<Articulo> _articulos;

        public DialogoEntradaAlmacen()
        {
            InitializeComponent();
            _context = new RectivoContext();
        }

        private async void DialEntradaAlmacen_Loaded(object sender, RoutedEventArgs e)
        {
            _articulos = await _context.Articulos
                .Where(a =>
                    a.Codigo.StartsWith("PS") ||
                    a.Codigo.StartsWith("PT") ||
                    a.Codigo.StartsWith("HE") ||
                    a.Codigo.StartsWith("MP"))
                .ToListAsync();

            // Códigos
            cmbCodigo.ItemsSource = _articulos;
            cmbCodigo.DisplayMemberPath = "Codigo";

            // Descripción 1 únicas
            cmbDescrip1.ItemsSource = _articulos
                .Select(a => a.Descrip)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Descripción 2 únicas
            cmbDescrip2.ItemsSource = _articulos
                .Select(a => a.Descrip2)
                .Where(d2 => !string.IsNullOrEmpty(d2))
                .Distinct()
                .OrderBy(d2 => d2)
                .ToList();
        }


        private void cmbDescrip1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDescrip1.SelectedItem is string descrip1)
            {
                // Filtrar las descripciones 2 válidas para esa descripción 1
                var opcionesDescrip2 = _articulos
                    .Where(a => a.Descrip == descrip1 && !string.IsNullOrEmpty(a.Descrip2))
                    .Select(a => a.Descrip2)
                    .Distinct()
                    .OrderBy(d2 => d2)
                    .ToList();

                cmbDescrip2.ItemsSource = opcionesDescrip2;
                cmbDescrip2.SelectedIndex = -1;
            }
        }

        private void cmbDescrip2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDescrip1.SelectedItem is string descrip1 &&
                cmbDescrip2.SelectedItem is string descrip2)
            {
                // Filtrar códigos que coincidan con la combinación de descrip1 + descrip2
                var coincidencias = _articulos
                    .Where(a => a.Descrip == descrip1 && a.Descrip2 == descrip2)
                    .ToList();

                cmbCodigo.ItemsSource = coincidencias;
                cmbCodigo.SelectedItem = coincidencias.FirstOrDefault();
            }
        }

        private void cmbCodigo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCodigo.SelectedItem is Articulo seleccionado)
            {
                // Al elegir un código, sincronizar descripciones
                cmbDescrip1.SelectedItem = seleccionado.Descrip;
                cmbDescrip2.SelectedItem = seleccionado.Descrip2;
            }
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                // Recuperar la ventana principal que ya está abierta
                var main = Application.Current.Windows
                    .OfType<MainWindow>()
                    .FirstOrDefault();

                if (main != null)
                {
                    // Traerla al frente
                    main.Activate();
                }

                // Cerrar el diálogo actual
                this.Close();
            }
        }
        private async void btnAnadirAlmacen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validar selección de artículo
                if (cmbCodigo.SelectedItem is not Articulo articuloSeleccionado)
                {
                    MessageBox.Show("Debes seleccionar un artículo válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Validar cantidad
                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Introduce una cantidad válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Validar ubicación
                string pasillo = txtPasillo.Text.Trim();
                string estanteria = txtEstanteria.Text.Trim();
                string hueco = txtHueco.Text.Trim();

                if (string.IsNullOrEmpty(pasillo) || string.IsNullOrEmpty(estanteria) || string.IsNullOrEmpty(hueco))
                {
                    MessageBox.Show("Debes indicar pasillo, estantería y hueco.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Convertir estantería y hueco a números
                int? estanteriaNum = int.TryParse(estanteria, out var est) ? est : null;
                int? huecoNum = int.TryParse(hueco, out var hue) ? hue : null;

                // 4. Buscar o crear ubicación
                var ubicacion = await _context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                        u.LetraPasillo == pasillo &&
                        u.NumeroEstanteria == estanteriaNum &&
                        u.Numero == huecoNum);

                if (ubicacion == null)
                {
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = pasillo,
                        NumeroEstanteria = estanteriaNum,
                        Numero = huecoNum
                    };
                    _context.Ubicacion.Add(ubicacion);
                    await _context.SaveChangesAsync();
                }

                // 5. Actualizar artículo
                articuloSeleccionado.Stock = (articuloSeleccionado.Stock ?? 0) + cantidad;
                articuloSeleccionado.IdUbicacion = ubicacion.IdUbicacion;

                _context.Articulos.Update(articuloSeleccionado);
                await _context.SaveChangesAsync();

                // 6. Confirmación
                MessageBox.Show(
                    $"Se añadieron {cantidad} unidades del artículo {articuloSeleccionado.Codigo} " +
                    $"al pasillo {pasillo}, estantería {estanteria}, hueco {hueco}.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 7. Limpiar campos
                txtCantidad.Clear();
                txtPasillo.Clear();
                txtEstanteria.Clear();
                txtHueco.Clear();
                cmbCodigo.SelectedIndex = -1;
                cmbDescrip1.SelectedIndex = -1;
                cmbDescrip2.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al añadir al almacén: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
