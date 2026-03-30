using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

[Table("empleado")]
public partial class Empleado : ValidatableViewModel
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    private string? _apellidos;
    [Column("APELLIDOS")]
    [StringLength(50)]
    [Required(ErrorMessage = "Los Apellidos son obligatorios")]
    public string? Apellidos
    {
        get => _apellidos;
        set => SetProperty(ref _apellidos, value);
    }

    private string? _nombre;
    [Column("NOMBRE")]
    [StringLength(50)]
    [Required(ErrorMessage = "El Nombre es obligatorio")]
    public string? Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    private string? _dni;
    [Column("DNI")]
    [StringLength(9)]
    [Required(ErrorMessage = "El DNI es obligatorio")]
    public string? Dni
    {
        get => _dni;
        set => SetProperty(ref _dni, value);
    }

    private string? _username;
    [Column("USERNAME")]
    [StringLength(50)]
    [Required(ErrorMessage = "El Usuario es obligatorio")]
    public string? Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string? _password;
    [Column("PASSWORD")]
    [StringLength(255)]
    [Required(ErrorMessage = "El Password es obligatorio")]
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    [Column("ID_ROL")]
    public int? IdRol { get; set; }

    private Rol? _rol;
    [ForeignKey("IdRol")]
    public virtual Rol? Rol
    {
        get => _rol;
        set => SetProperty(ref _rol, value);
    }

    [Column("ESTADO")]
    public string Estado { get; set; } = "activo";

    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();
}