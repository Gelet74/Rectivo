using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using recTivo.MVVM.Base;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.Dialogos
{
    /// <summary>
    /// Lógica de interacción para DialogoEntradaAlmacen.xaml
    /// </summary>
    public partial class DialogoEntradaAlmacen : Window
    {
        private RectivoContext _context;
        private List<Articulo> _articulos;
       
        public DialogoEntradaAlmacen(RectivoContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void DialEntradaAlmacen_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MVBase vm)
            {
                this.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(vm.OnErrorEvent));
            }


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

        private void txtPasillo_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == null)
                return;

            int caret = tb.CaretIndex; 

            string mayus = tb.Text.ToUpper();

            if (tb.Text != mayus)
            {
                tb.Text = mayus;
                tb.CaretIndex = caret; 
            }
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


        private async void btnAnadirAlmacen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                if (cmbCodigo.SelectedItem is not Articulo articuloSeleccionado)
                {
                    MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo válido.");
                    return;
                }

               
                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Introduce una cantidad válida.");
                    return;
                }

               
                string pasillo = txtPasillo.Text.Trim();
                string estanteria = txtEstanteria.Text.Trim();
                string hueco = txtHueco.Text.Trim();

                if (string.IsNullOrEmpty(pasillo) || string.IsNullOrEmpty(estanteria) || string.IsNullOrEmpty(hueco))
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Debes indicar pasillo, estantería y hueco.");
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
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = pasillo,
                        NumeroEstanteria = estanteriaNum,
                        Numero = huecoNum
                    };
                    _context.Ubicacion.Add(ubicacion);
                    await _context.SaveChangesAsync();
                }

               
                articuloSeleccionado.Stock = (articuloSeleccionado.Stock ?? 0) + cantidad;

                if (articuloSeleccionado.Stock == 0) 
                { 
                    articuloSeleccionado.IdUbicacion = null;
                }
                else
                {
                    articuloSeleccionado.IdUbicacion = ubicacion.IdUbicacion;
                }
                _context.Articulos.Update(articuloSeleccionado); 
                await _context.SaveChangesAsync();
                articuloSeleccionado.IdUbicacion = ubicacion.IdUbicacion;

                _context.Articulos.Update(articuloSeleccionado);
                await _context.SaveChangesAsync();

               
                MensajeInformacion.Mostrar("ÉXITO",
                    $"Se añadieron {cantidad} unidades del artículo {articuloSeleccionado.Codigo} " +
                    $"al pasillo {pasillo}, estantería {estanteria}, hueco {hueco}.");

                
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
                MensajeError.Mostrar("ERROR", $"Error al añadir al almacén: {ex.Message}");
            }
        }
    }
}
