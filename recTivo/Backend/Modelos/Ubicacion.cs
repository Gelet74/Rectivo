using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

[Table("ubicacion")]
public class Ubicacion
{
    [Key]
    [Column("ID_UBICACION")]
    public int IdUbicacion { get; set; }

    [Column("NUMERO")]
    public int? Numero { get; set; }

    [Column("LETRA_PASILLO")]
    public string? LetraPasillo { get; set; }

    [Column("NUMERO_ESTANTERIA")]
    public int? NumeroEstanteria { get; set; }

    [Column("CANTIDAD")]
    public int Cantidad { get; set; }

    [Column("ID_ARTICULO")]
    public int? IdArticulo { get; set; }

    [ForeignKey("IdArticulo")]
    public virtual Articulo? Articulo { get; set; }
}