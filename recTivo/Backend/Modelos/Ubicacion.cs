using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

public partial class Ubicacion
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

    public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
}
