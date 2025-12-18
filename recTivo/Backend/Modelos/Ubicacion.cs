using System.Collections.Generic;

namespace recTivo.Backend.Modelos;

public class Ubicacion
{
    public int IdUbicacion { get; set; }

    public int? Numero { get; set; }
    public string? LetraPasillo { get; set; }
    public int? NumeroEstanteria { get; set; }

    public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
}
