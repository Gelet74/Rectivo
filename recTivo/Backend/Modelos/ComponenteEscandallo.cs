namespace recTivo.Backend.Modelos
{
    public class ComponenteEscandallo
    {
        public int IdComponente { get; set; }
        public int IdEscandallo { get; set; }
        public string CodigoArticulo { get; set; }
        public string Descripcion { get; set; }
        public string Descripcion2 { get; set; }
        public double Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public Escandallo Escandallo { get; set; }
    }
}
