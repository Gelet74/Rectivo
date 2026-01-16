using recTivo.MVVM.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    public class ComponenteEscandallo : ValidatableViewModel
    {
        public int IdComponente { get; set; }
        public int IdEscandallo { get; set; }

        // Código del artículo componente (clave real)
        public string CodigoArticulo { get; set; }

        public string Descripcion { get; set; }

        [Column("Descrip2")]
        public string? Descripcion2 { get; set; }

        public double? Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }

        public Escandallo Escandallo { get; set; }

        // Código del padre (si es null → es raíz)
        public String? CodigoComponentePadre { get; set; }

        // Hijos en memoria (no en BD)
        [NotMapped]       
        public ObservableCollection<ComponenteEscandallo> Hijos { get; set; } = new();


        // Nombre opcional para mostrar
        [NotMapped]
        public string? NombreComponente { get; set; }

    
    }
}
