using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    [Keyless]
    [Table("vista_articulos_ubicacion")]
    public class VistaArticuloUbicacion
    {
        [Column("ID_ARTICULO")]
        public int IdArticulo { get; set; }

        [Column("CODIGO")]
        [StringLength(10)]
        public string Codigo { get; set; } = null!;

        [Column("DESCRIP")]
        [StringLength(50)]
        public string Descrip { get; set; } = null!;

        [Column("DESCRIP2")]
        [StringLength(50)]
        public string? Descrip2 { get; set; }

        [Column("STOCK")]
        public int? Stock { get; set; }

        [Column("PVP")]
        public double? Pvp { get; set; }

        [Column("LETRA_PASILLO")]
        [StringLength(10)]
        public string? LetraPasillo { get; set; }

        [Column("NUMERO_ESTANTERIA")]
        public int? NumeroEstanteria { get; set; }

        [Column("HUECO")]
        public int? Hueco { get; set; }
    }
}
