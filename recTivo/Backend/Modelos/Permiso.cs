using System;
using System.Collections.Generic;

namespace recTivo.Backend.Modelos;

public partial class Permiso
{
    public int Id { get; set; }

    public string NombrePermiso { get; set; } = null!;

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
