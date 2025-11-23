using System;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos;

public partial class Orden
{
    public int IdOrden { get; set; }

    public string Codigo { get; set; } = null!;

    public int Cantidad { get; set; }

    public DateTime? FechaFin { get; set; }

    public int IdEmpleado { get; set; }

    public int IdArticulo { get; set; }

    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
