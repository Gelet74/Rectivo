using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos;

[Table("rol")]
public partial class Rol
{
    [Key]
    [Column("ID")]
    public int Id { get; set; } // PK, NOT NULL, AUTO_INCREMENT

    [Column("NOMBRE_ROL")]
    [StringLength(50)]
    public string NombreRol { get; set; } = null!;

    // 🔹 Relación con Empleado
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    // 🔹 Relación con Permiso
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
