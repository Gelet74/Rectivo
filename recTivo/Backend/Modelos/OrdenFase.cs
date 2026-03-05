using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos;

public class OrdenFase
{
    public int IdOrdenFase { get; set; }
    public int IdOrden { get; set; }

    /// <summary>Código del artículo de fase: 01x, 02x o 03x</summary>
    public string CodigoFase { get; set; } = null!;

    /// <summary>1 = fase 01x, 2 = fase 02x, 3 = fase 03x</summary>
    public int NumeroFase { get; set; }

    public int CantidadEntrada { get; set; }
    public int? CantidadOK { get; set; }
    public int? CantidadDefecto { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? IdEmpleado { get; set; }
    public string Estado { get; set; } = nameof(EstadoOrden.Pendiente);

    // ── Propiedades calculadas ──
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

    [NotMapped]
    public string NombreFaseTexto
    {
        get
        {
            if (CodigoFase == "AGRUPAMIENTO") return "FASE 1 · AGRUPAMIENTO";
            return CodigoFase?.Substring(0, 2) switch
            {
                "01" => "FASE 1 · SECCIONADORA",
                "02" => "FASE 2 · CANTEADORA",
                "03" => "FASE 3 · MECANIZADO",
                _ => $"FASE {NumeroFase}"
            };
        }
    }

    [NotMapped]
    public string NombreFase => NombreFaseTexto;

    // ── Navegación ──
    public virtual Orden OrdenNavigation { get; set; } = null!;
    public virtual Empleado? EmpleadoNavigation { get; set; }
}