using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos
{

    [Table("articulo")]
    public class Articulo
    {
        [Key]
        [Column("id_articulo")]
        public int IdArticulo { get; set; } // PK, NOT NULL, AUTO_INCREMENT

        [Column("codigo")]
        [StringLength(10)]
        public string Codigo { get; set; } = null!; // NOT NULL, UNIQUE

        [Column("descrip")]
        [StringLength(50)]
        public string Descrip { get; set; } = null!; // NOT NULL

        [Column("descrip2")]
        [StringLength(50)]
        public string? Descrip2 { get; set; } // NULL

        [Column("stock")]
        public int? Stock { get; set; } = 0; // NULL, DEFAULT 0

        [Column("pvp")]
        public double? Pvp { get; set; } // NULL

        [Column("id_ubicacion")]
        public int? IdUbicacion { get; set; } // NULL

        // 🔹 Relación con Ubicacion
        [ForeignKey("IdUbicacion")]
        public virtual Ubicacion? Ubicacion { get; set; }

        // 🔹 Relación con ClienteHasArticulo (si aplica)
        public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();
    }
}
