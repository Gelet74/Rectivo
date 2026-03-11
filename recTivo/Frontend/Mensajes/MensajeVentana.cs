using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace di.proyecto.clase._2025.Frontend.Mensajes
{
    public enum TipoMensaje
    {
        Informacion,
        Advertencia,
        Error
    }

    public abstract class MensajeBase
    {
        public string Titulo { get; set; }
        public string Cuerpo { get; set; }
        public BitmapImage? Imagen { get; set; }
        public SolidColorBrush? ColorDistintivo { get; private set; }
        public TipoMensaje Tipo { get; set; }
        public int TiempoAutoclose { get; set; }

        protected MensajeBase(string titulo, string cuerpo, TipoMensaje tipo, int tiempoAutoclose = 0)
        {
            Titulo = titulo;
            Cuerpo = cuerpo;
            Tipo = tipo;
            TiempoAutoclose = tiempoAutoclose;
            ConfigurarColorEIcono();
        }

        private void ConfigurarColorEIcono()
        {
            switch (Tipo)
            {
                case TipoMensaje.Informacion:
                    ColorDistintivo = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                    Imagen = CargarIcono("exito");
                    break;

                case TipoMensaje.Advertencia:
                    ColorDistintivo = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    Imagen = CargarIcono("warning");
                    break;

                case TipoMensaje.Error:
                    ColorDistintivo = new SolidColorBrush(Color.FromRgb(255, 214, 0));
                    Imagen = CargarIcono("error");
                    break;
            }
        }

        private BitmapImage? CargarIcono(string nombreIcono)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/Recursos/Iconos/{nombreIcono}.jpg",
                    UriKind.RelativeOrAbsolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }

        public abstract void Mostrar();
        public abstract void Cerrar();
    }

    public abstract class MensajeVentana : MensajeBase
    {
        protected VentanaMensaje? _ventana;

        protected MensajeVentana(string titulo, string cuerpo, TipoMensaje tipo, int tiempoAutoclose = 0)
            : base(titulo, cuerpo, tipo, tiempoAutoclose)
        {
        }

        protected void MostrarVentana()
        {
            _ventana = new VentanaMensaje(this);
            _ventana.ShowDialog();

            if (TiempoAutoclose > 0)
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(TiempoAutoclose * 1000)
                };
                timer.Tick += (s, e) => { timer.Stop(); Cerrar(); };
                timer.Start();
            }
        }

        public override void Cerrar() => _ventana?.Close();
    }

    public class MensajeInformacion : MensajeVentana
    {
        private MensajeInformacion(string titulo, string cuerpo, int tiempoAutoclose = 0)
            : base(titulo, cuerpo, TipoMensaje.Informacion, tiempoAutoclose) { }

        public static void Mostrar(string titulo, string cuerpo, int tiempoAutoclose = 0)
            => new MensajeInformacion(titulo, cuerpo, tiempoAutoclose).MostrarVentana();

        public override void Mostrar() => MostrarVentana();
    }

    public class MensajeAdvertencia : MensajeVentana
    {
        private MensajeAdvertencia(string titulo, string cuerpo, int tiempoAutoclose = 0)
            : base(titulo, cuerpo, TipoMensaje.Advertencia, tiempoAutoclose) { }

        public static void Mostrar(string titulo, string cuerpo, int tiempoAutoclose = 0)
            => new MensajeAdvertencia(titulo, cuerpo, tiempoAutoclose).MostrarVentana();

        public override void Mostrar() => MostrarVentana();
    }

    public class MensajeError : MensajeVentana
    {
        private MensajeError(string titulo, string cuerpo, int tiempoAutoclose = 0)
            : base(titulo, cuerpo, TipoMensaje.Error, tiempoAutoclose) { }

        public static void Mostrar(string titulo, string cuerpo, int tiempoAutoclose = 0)
            => new MensajeError(titulo, cuerpo, tiempoAutoclose).MostrarVentana();

        public override void Mostrar() => MostrarVentana();
    }
}