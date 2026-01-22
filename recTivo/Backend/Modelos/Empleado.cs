using recTivo.MVVM.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

[Table("empleado")]
public partial class Empleado : ValidatableViewModel
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("APELLIDOS")]
    [StringLength(50)]
    [Required(ErrorMessage = "Los Apellidos son obligatorios")]
    public string? Apellidos { get; set; }

    [Column("NOMBRE")]
    [StringLength(50)]
    [Required(ErrorMessage = "El Nombre es obligatorio")]
    public string? Nombre { get; set; }

    [Column("DNI")]
    [StringLength(9)]
    [Required(ErrorMessage = "El DNI es obligatorio")]
    public string? Dni { get; set; }

    [Column("USERNAME")]
    [StringLength(50)]
    [Required(ErrorMessage = "El Usuario es obligatorio")]
    public string? Username { get; set; }

    [Column("PASSWORD")]
    [StringLength(50)]
    [Required(ErrorMessage = "El Password es obligatorio")]
    public string? Password { get; set; }

    [Column("ID_ROL")]
    public int? IdRol { get; set; }

    [ForeignKey("IdRol")]
    public virtual Rol? Rol { get; set; }

    [Column("ESTADO")]
    public string Estado { get; set; } = "activo";

    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellidos}";





    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}
