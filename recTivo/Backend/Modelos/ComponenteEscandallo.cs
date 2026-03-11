using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    [Table("componenteescandallo")]
    public class ComponenteEscandallo
    {
        [Key]
        [Column("IdComponente")]
        public int IdComponente { get; set; }

        [Column("IdEscandallo")]
        public int IdEscandallo { get; set; }

        [Column("CodigoArticulo")]
        [MaxLength(10)]
        public string CodigoArticulo { get; set; } = null!;

        [Column("Cantidad")]
        public decimal? Cantidad { get; set; }

        [Column("PrecioUnitario")]
        public decimal? PrecioUnitario { get; set; }

        [Column("CodigoComponentePadre")]
        [MaxLength(10)]
        public string? CodigoComponentePadre { get; set; }

        // ⭐ NO están en la BD, se cargan en memoria
        [NotMapped]
        public string? Descripcion { get; set; }

        [NotMapped]
        public string? Descripcion2 { get; set; }

        // Navegación
        [NotMapped]
        public Escandallo? Escandallo { get; set; }

        [NotMapped]
        public ObservableCollection<ComponenteEscandallo>? Hijos { get; set; }

        [NotMapped]
        public string? NombreComponente { get; set; }
    }
}