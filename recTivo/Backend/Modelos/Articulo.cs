using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos
{

    public class Articulo
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; } = null!;
        public string Descrip { get; set; } = null!;
        public string? Descrip2 { get; set; }
        public int? Stock { get; set; } = 0;
        public double? Pvp { get; set; }
        public int? IdUbicacion { get; set; }

        public virtual Ubicacion? Ubicacion { get; set; }
        public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();
    }
}
