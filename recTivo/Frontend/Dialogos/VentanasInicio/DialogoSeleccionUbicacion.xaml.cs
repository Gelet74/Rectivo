using recTivo.Backend.Modelos;
using System.Windows;

namespace recTivo.Frontend.Dialogos.VentanasInicio
{
    // ViewModel ligero para cada fila de la lista
    public class FilaUbicacion
    {
        public Ubicacion Ubicacion { get; set; } = null!;
        public string Etiqueta => $"Pasillo {Ubicacion.LetraPasillo}{Ubicacion.Numero}  —  Estantería {Ubicacion.NumeroEstanteria}";
        public string StockTexto => $"{Ubicacion.Cantidad} uds.";
    }

    public partial class DialogoSeleccionUbicacion : Window
    {
        // Resultado: ubicación elegida, o null si canceló
        public Ubicacion? UbicacionElegida { get; private set; }
        public bool Cancelado { get; private set; }

        public DialogoSeleccionUbicacion(
            string codigoArticulo,
            string descripcion,
            int cantidadNecesaria,
            List<Ubicacion> ubicaciones)
        {
            InitializeComponent();

            txbArticulo.Text = $"{codigoArticulo}  —  {descripcion}";
            txbNecesario.Text = $"Cantidad a descontar: {cantidadNecesaria} uds.";

            lstUbicaciones.ItemsSource = ubicaciones
                .Select(u => new FilaUbicacion { Ubicacion = u })
                .ToList();

            if (lstUbicaciones.Items.Count > 0)
                lstUbicaciones.SelectedIndex = 0;
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (lstUbicaciones.SelectedItem is FilaUbicacion fila)
            {
                UbicacionElegida = fila.Ubicacion;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Selecciona una ubicación de la lista.",
                    "VENTAS", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Cancelado = true;
            DialogResult = false;
        }
    }
}