using recTivo.Backend.Modelos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("rol")]
public partial class Rol
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NOMBRE_ROL")]
    [StringLength(50)]
    public string NombreRol { get; set; } = null!;

    [NotMapped]
    public string Nombre => NombreRol;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
