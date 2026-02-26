using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // ← AÑADIDO

namespace recTivo.Backend.Modelos;

// ← AÑADIDO: enum fuera de la clase, mismo fichero
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

    // ← AÑADIDO: persiste en BD como string ("Pendiente" por defecto)
    public string Estado { get; set; } = nameof(EstadoOrden.Pendiente);

    // ← AÑADIDO: para trabajar con el enum en código
    [NotMapped]
    public EstadoOrden EstadoEnum
    {
        get => Enum.TryParse<EstadoOrden>(Estado, out var e) ? e : EstadoOrden.Pendiente;
        set => Estado = value.ToString();
    }

    // ← AÑADIDO: para mostrar en UI ("En curso" en lugar de "EnCurso")
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
}