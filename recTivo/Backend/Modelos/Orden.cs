using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

public enum EstadoOrden
{
    Pendiente,
    EnCurso,
    Cerrada
}

public partial class Orden
{
    public int IdOrden { get; set; }

    public string Codigo { get; set; } = null!;

    public int Cantidad { get; set; }

    public DateTime? FechaFin { get; set; }

    public int IdEmpleado { get; set; }

    public int IdArticulo { get; set; }

    /// <summary>
    /// Estado de la orden: Pendiente | EnCurso | Cerrada
    /// Se almacena como string en BD (columna Estado VARCHAR(20))
    /// </summary>
    public string Estado { get; set; } = nameof(EstadoOrden.Pendiente);

    [NotMapped]
    public EstadoOrden EstadoEnum
    {
        get => Enum.TryParse<EstadoOrden>(Estado, out var e) ? e : EstadoOrden.Pendiente;
        set => Estado = value.ToString();
    }

    [NotMapped]
    public string EstadoTexto => EstadoEnum switch
    {
        EstadoOrden.Pendiente => "Pendiente",
        EstadoOrden.EnCurso => "En curso",
        EstadoOrden.Cerrada => "Cerrada",
        _ => Estado
    };

    public virtual Articulo IdArticuloNavigation { get; set; } = null!;
    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
    public virtual ICollection<OrdenFase> Fases { get; set; } = new List<OrdenFase>();
}
