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
    /// Lógica de interacción para DialogoSalidaAlmacen.xaml
    /// </summary>
    public partial class DialogoSalidaAlmacen : Window
    {
        private RectivoContext _context;
        private List<Articulo> _articulos;
        public DialogoSalidaAlmacen()
        {
            InitializeComponent();
            _context = new RectivoContext();
        }

        private async void DialSalidaAlmacen_Loaded(object sender, RoutedEventArgs e)
        {
            _articulos = await _context.Articulos
                .Where(a =>
                    a.Codigo.StartsWith("PS") ||
                    a.Codigo.StartsWith("PT") ||
                    a.Codigo.StartsWith("HE") ||
                    a.Codigo.StartsWith("MP"))
                .ToListAsync();

            
            cmbCodigo.ItemsSource = _articulos;
            cmbCodigo.DisplayMemberPath = "Codigo";

            
            cmbDescrip1.ItemsSource = _articulos
                .Select(a => a.Descrip)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            
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
                cmbDescrip1.SelectedItem = seleccionado.Descrip;
                cmbDescrip2.SelectedItem = seleccionado.Descrip2;
            }
        }


        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == System.Windows.Input.Key.Escape)
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

        private async void btnRestarAlmacen_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                
                if (cmbCodigo.SelectedItem is not Articulo articuloSeleccionado)
                {
                    MessageBox.Show("Debes seleccionar un artículo válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

               
                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Introduce una cantidad válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                
                string pasillo = txtPasillo.Text.Trim();
                string estanteria = txtEstanteria.Text.Trim();
                string hueco = txtHueco.Text.Trim();

                if (string.IsNullOrEmpty(pasillo) || string.IsNullOrEmpty(estanteria) || string.IsNullOrEmpty(hueco))
                {
                    MessageBox.Show("Debes indicar pasillo, estantería y hueco.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
               
                int? estanteriaNum = int.TryParse(estanteria, out var est) ? est : null;
                int? huecoNum = int.TryParse(hueco, out var hue) ? hue : null;


                var ubicacion = await _context.Ubicacion
                    .FirstOrDefaultAsync(u =>
                    u.LetraPasillo == pasillo &&
                    u.NumeroEstanteria == estanteriaNum &&
                    u.Numero == huecoNum);

                if (ubicacion == null)
                {
                    throw new InvalidOperationException(
                        $"La ubicación {pasillo}-{estanteria}-{hueco} no existe en el sistema."
                    );
                }

                if ((articuloSeleccionado.Stock ?? 0) < cantidad)
                {
                    MessageBox.Show(
                        $"No hay suficiente stock del artículo {articuloSeleccionado.Codigo}. " +
                        $"Stock actual: {articuloSeleccionado.Stock ?? 0}, " +
                        $"Cantidad solicitada: {cantidad}.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                articuloSeleccionado.Stock = (articuloSeleccionado.Stock ?? 0) - cantidad;
                articuloSeleccionado.IdUbicacion = ubicacion.IdUbicacion;

                _context.Articulos.Update(articuloSeleccionado);
                await _context.SaveChangesAsync();

                
                MessageBox.Show(
                    $"Se restaron {cantidad} unidades del artículo {articuloSeleccionado.Codigo} " +
                    $"del pasillo {pasillo}, estantería {estanteria}, hueco {hueco}.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                
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
                MessageBox.Show($"Error al restarr al almacén: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

