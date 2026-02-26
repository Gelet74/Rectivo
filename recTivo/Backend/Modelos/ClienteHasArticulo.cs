using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    [Table("cliente_has_articulo")]
    public class ClienteHasArticulo
    {
        [Column("cliente_IDCLIENTE")]
        public int ClienteIdcliente { get; set; }

        [Column("articulo_CODIGO")]
        public string ArticuloCodigo { get; set; } = null!;

        public virtual Cliente? Cliente { get; set; }
        public virtual Articulo? Articulo { get; set; }
    }
}