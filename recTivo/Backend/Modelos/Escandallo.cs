namespace recTivo.Backend.Modelos
{
    public class Escandallo
    {
        public int IdEscandallo { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion2 { get; set; }

        public ICollection<ComponenteEscandallo> Componentes { get; set; } = new List<ComponenteEscandallo>();
    }
}
