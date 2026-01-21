using recTivo.Backend.Modelos;
using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Cliente : ValidatableViewModel
{
    public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();
    
    [Key]
    [Column("IDCLIENTE")]
    public int IdCliente { get; set; }

    [Column("NOMBRE")]
    public string? Nombre { get; set; }

    [Column("APELLIDO1")]
    public string? Apellido1 { get; set; }

    [Column("APELLIDO2")]
    public string? Apellido2 { get; set; }

    [Column("NUM_FACTURA")]
    public int? NumFactura { get; set; }

    [Column("NUM_PEDIDO")]
    public int? NumPedido { get; set; }

    [Column("DNI")]
    public string? Dni { get; set; }

    [Column("TELEFONO")]
    public string? Telefono { get; set; }

    [Column("username")]
    public string Usuario { get; set; }

    [Column("password")]
    public string Password { get; set; }


    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido1} {Apellido2}".Trim();

}
