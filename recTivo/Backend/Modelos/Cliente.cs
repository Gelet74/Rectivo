using recTivo.Backend.Modelos;
using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("cliente")]
public class Cliente : ValidatableViewModel
{
    public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();

    [Key]
    [Column("IDCLIENTE")]
    public int IdCliente { get; set; }

    private string? _nombre;
    [Column("NOMBRE")]
    [Required(ErrorMessage = "El Nombre es obligatorio")]
    public string? Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    private string? _apellido1;
    [Column("APELLIDO1")]
    [Required(ErrorMessage = "El primer apellido es obligatorio")]
    public string? Apellido1
    {
        get => _apellido1;
        set => SetProperty(ref _apellido1, value);
    }

    private string? _apellido2;
    [Column("APELLIDO2")]
    public string? Apellido2
    {
        get => _apellido2;
        set => SetProperty(ref _apellido2, value);
    }

    [Column("NUM_FACTURA")]
    public int? NumFactura { get; set; }

    [Column("NUM_PEDIDO")]
    public int? NumPedido { get; set; }

    private string? _dni;
    [Column("DNI")]
    [Required(ErrorMessage = "El DNI es obligatorio")]
    public string? Dni
    {
        get => _dni;
        set => SetProperty(ref _dni, value);
    }

    private string? _telefono;
    [Column("TELEFONO")]
    public string? Telefono
    {
        get => _telefono;
        set => SetProperty(ref _telefono, value);
    }

    private string _usuario = null!;
    [Column("username")]
    [Required(ErrorMessage = "El Usuario es obligatorio")]
    public string Usuario
    {
        get => _usuario;
        set => SetProperty(ref _usuario, value);
    }

    private string _password = null!;
    [Column("password")]
    [Required(ErrorMessage = "La Contraseña es obligatoria")]
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido1} {Apellido2}".Trim();
}