using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos;

[Table("empleado")]
public partial class Empleado
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("APELLIDOS")]
    public string? Apellidos { get; set; }

    [Column("NOMBRE")]
    public string? Nombre { get; set; }

    [Column("DNI")]
    public string? Dni { get; set; }

    [Column("USERNAME")]
    public string? Username { get; set; }

    [Column("PASSWORD")]
    public string? Password { get; set; }

    [Column("ID_ROL")]
    public int? IdRol { get; set; }

    [ForeignKey("IdRol")]
    public virtual Rol? Rol { get; set; }

    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}
