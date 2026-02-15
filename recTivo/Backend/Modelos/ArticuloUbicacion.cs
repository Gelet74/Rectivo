using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    [Table("articuloubicacion")]
    public class ArticuloUbicacion
    {
        [Key]
        [Column("id_articulo_ubicacion")]
        public int IdArticuloUbicacion { get; set; }

        [Column("id_articulo")]
        public int IdArticulo { get; set; }

        [Column("id_ubicacion")]
        public int IdUbicacion { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        public Articulo Articulo { get; set; }
        public Ubicacion Ubicacion { get; set; }
    }

}