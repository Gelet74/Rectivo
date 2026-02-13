using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{

    public class Articulo : ValidatableViewModel
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = null!;
        [Column("descripcion")]
        public string descrip { get; set; } = null!;
        [Column("descripcion2")]
        public string? descrip2 { get; set; }
        public int? Stock { get; set; } = 0;
        public double? Pvp { get; set; }
        public int? IdUbicacion { get; set; }

        [Column("precio_compra")]
        public decimal? PrecioCompra { get; set; }

        public virtual Ubicacion? Ubicacion { get; set; }
        public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();
    }
}
